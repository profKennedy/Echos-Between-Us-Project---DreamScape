
using System.Collections;
using UnityEngine;
using Cinemachine;

public class EventoEncogimientoAula : MonoBehaviour
{
    [Header("Presentacion del fragmento")]
    [SerializeField] private CinemachineVirtualCamera camaraFragmentoAula;
    [SerializeField] private float duracionPlanoFragmento = 2f;

    [Header("Referencias")]
    [SerializeField] private EfectoEncogimiento efectoEncogimiento;
    [SerializeField] private MovimientoPersonaje movimientoDani;
    [SerializeField] private GestorEntradas gestorEntradas;

    [Header("Configuracion")]
    [SerializeField] private float tiempoMaximo = 5f;

    [Header("Puerta del aula")]
    [SerializeField] private Puerta puertaAula;
    [SerializeField] private float retrasoCierre = 0.5f;

    [Header("Fantasma del aula")]
    [SerializeField] private GameObject fantasmaAula;

    [Header("Camaras Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera camaraCinematicaAula;
    [SerializeField] private CinemachineFreeLook camaraDani;
    [SerializeField] private float esperaCambioCamara = 0.5f;
    [SerializeField] private float esperaRegresoCamara = 0.5f;

    [Header("Final del puzzle")]
    [SerializeField] private float retrasoApertura = 1f;

    [Tooltip("Opcional: ID que se registra al completar el aula.")]
    [SerializeField] private string idPuzzle = "PuzzleAula";

    [Header("Ambientacion del aula")]
    [SerializeField] private GameObject elementosNegatividad;

    [Header("Efectos al completar el puzzle")]
    [SerializeField] private DesvanecerObjetosAula objetosNegatividad;

    private bool puzzleCompletado = false;

    private bool eventoIniciado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (eventoIniciado)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (efectoEncogimiento == null)
        {
            Debug.LogError(
                "EventoEncogimientoAula: falta EfectoEncogimiento.",
                this
            );
            return;
        }

        StartCoroutine(SecuenciaEncogimiento());
    }

    private IEnumerator SecuenciaEncogimiento()
    {
        eventoIniciado = true;

        // 1. Bloquear el movimiento de Dani.
        if (movimientoDani != null)
            movimientoDani.Bloquear(true);

        // Desactivar entradas mientras dura la cinemática.
        if (gestorEntradas != null)
            gestorEntradas.enabled = false;

        // 2. Activar la cámara cinematográfica.
        if (camaraCinematicaAula != null)
        {
            camaraCinematicaAula.gameObject.SetActive(true);
            camaraCinematicaAula.Priority = 20;
        }

        if (camaraDani != null)
            camaraDani.Priority = 10;

        // Esperar a que Cinemachine cambie de cámara.
        yield return new WaitForSeconds(esperaCambioCamara);

        // 3. Esperar antes de cerrar la puerta.
        yield return new WaitForSeconds(retrasoCierre);

        // 4. Cerrar la puerta detrás de Dani.
        if (puertaAula != null)
            puertaAula.CerrarPorEvento();

        // 5. Comenzar el encogimiento.
        efectoEncogimiento.EncogerPorPuzzle();

        float tiempo = 0f;

        // Esperar hasta que Dani termine de encogerse.
        while (
            efectoEncogimiento.ObtenerFactorActual() >
            efectoEncogimiento.factorMinimo + 0.01f &&
            tiempo < tiempoMaximo
        )
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Mostrar el fragmento antes de devolver el control.
        if (camaraFragmentoAula != null)
        {
            camaraFragmentoAula.gameObject.SetActive(true);
            camaraFragmentoAula.Priority = 40;

            // Dejamos de mostrar la cámara del encogimiento.
            if (camaraCinematicaAula != null)
                camaraCinematicaAula.gameObject.SetActive(false);

            yield return new WaitForSeconds(duracionPlanoFragmento);

            // Desactivamos la cámara del fragmento.
            camaraFragmentoAula.gameObject.SetActive(false);
        }

        // 6. Regresar a la cámara normal.
        if (camaraDani != null)
            camaraDani.Priority = 30;

        if (camaraCinematicaAula != null)
            camaraCinematicaAula.gameObject.SetActive(false);

        if (fantasmaAula != null)
        {
            fantasmaAula.SetActive(true);
        }

        // Esperar la transición de Cinemachine.
        yield return new WaitForSeconds(esperaRegresoCamara);

        // 7. Restaurar controles.
        if (gestorEntradas != null)
            gestorEntradas.enabled = true;

        if (movimientoDani != null)
            movimientoDani.Bloquear(false);

        Debug.Log("Cinemática del aula finalizada. Dani está encogida.");
    }

    // Lo llama el fragmento cuando Dani termina de recogerlo.
    public void RegistrarFragmento()
    {
        if (!eventoIniciado || puzzleCompletado)
            return;

        puzzleCompletado = true;

        StartCoroutine(SecuenciaLiberacion());
    }

    private IEnumerator SecuenciaLiberacion()
    {
        Debug.Log("Puzzle del aula completado.");

        // 1. Desactivar el fantasma.
        if (fantasmaAula != null)
            fantasmaAula.SetActive(false);

        // Iniciar desaparición gradual de la decoración.
        if (objetosNegatividad != null)
            objetosNegatividad.IniciarDesaparicion();

        // 2. Dani recupera su tamaño original.
        if (efectoEncogimiento != null)
            efectoEncogimiento.RecuperarTamanoPorPuzzle();

        // 3. Registrar el puzzle completado.
        if (!string.IsNullOrEmpty(idPuzzle) &&
            GameProgressManager.Instancia != null)
        {
            GameProgressManager.Instancia.RegistrarPuzzleCompletado(idPuzzle);
        }

        // 4. Esperar a que Dani recupere su tamaño.
        float tiempo = 0f;

        while (efectoEncogimiento != null &&
               efectoEncogimiento.ObtenerFactorActual() < 0.99f &&
               tiempo < 5f)
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Pequeña pausa antes de abrir la puerta.
        yield return new WaitForSeconds(retrasoApertura);

        // 5. Abrir nuevamente la puerta.
        if (puertaAula != null)
            puertaAula.AbrirAlCompletarPuzzle();

        Debug.Log("Dani puede salir del aula.");
    }
}
