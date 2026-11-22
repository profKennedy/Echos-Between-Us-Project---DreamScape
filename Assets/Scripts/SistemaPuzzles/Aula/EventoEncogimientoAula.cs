
using System.Collections;
using UnityEngine;
using Cinemachine;

public class EventoEncogimientoAula : MonoBehaviour
{
    [Header("Presentacion de la llave")]
    [SerializeField] private CinemachineVirtualCamera camaraFragmentoAula;
    [SerializeField] private float duracionPlanoFragmento = 2f;

    [Header("Referencias")]
    [SerializeField] private EfectoEncogimiento efectoEncogimiento;
    [SerializeField] private MovimientoPersonaje movimientoDani;
    [SerializeField] private GestorEntradas gestorEntradas;
    [SerializeField] private InteraccionPersonaje interaccionDani;

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
    [SerializeField] private float esperaRegresoCamara = 1.5f;

    [Header("Camara de negatividad")]
    [SerializeField] private CinemachineVirtualCamera camaraNegatividad;
    [SerializeField] private float tiempoTransicionNegatividad = 1.5f;
    [SerializeField] private float tiempoObservandoNegatividad = 2f;
    [SerializeField] private float tiempoDesaparicionNegatividad = 3f;

    [Header("Final del puzzle")]
    [SerializeField] private float retrasoApertura = 1f;
    [SerializeField] private string idPuzzle = "PuzzleAula";

    [Header("Ambientacion del aula")]
    [SerializeField] private GameObject elementosNegatividad;

    [Header("Efectos al completar el puzzle")]
    [SerializeField] private DesvanecerObjetosAula objetosNegatividad;

    private bool puzzleCompletado = false;
    private bool eventoIniciado = false;
    private bool cinemáticaFinalActiva = false;

    private int prioridadOriginalDani;
    private int prioridadOriginalNegatividad;

    private void Start()
    {
        if (camaraDani != null)
            prioridadOriginalDani = camaraDani.Priority;

        if (camaraNegatividad != null)
        {
            prioridadOriginalNegatividad = camaraNegatividad.Priority;
            camaraNegatividad.gameObject.SetActive(false);
        }
    }

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

        // Bloquear movimiento durante el encogimiento.
        if (movimientoDani != null)
            movimientoDani.Bloquear(true);

        if (gestorEntradas != null)
            gestorEntradas.enabled = false;

        // Mostrar camara del encogimiento.
        if (camaraCinematicaAula != null)
        {
            camaraCinematicaAula.gameObject.SetActive(true);
            camaraCinematicaAula.Priority = 20;
        }

        if (camaraDani != null)
            camaraDani.Priority = 10;

        yield return new WaitForSeconds(esperaCambioCamara);

        yield return new WaitForSeconds(retrasoCierre);

        if (puertaAula != null)
            puertaAula.CerrarPorEvento();

        // Encoger a Dani.
        efectoEncogimiento.EncogerPorPuzzle();

        float tiempo = 0f;

        while (
            efectoEncogimiento.ObtenerFactorActual() >
            efectoEncogimiento.factorMinimo + 0.01f &&
            tiempo < tiempoMaximo
        )
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Mostrar la llave de biblioteca.
        if (camaraFragmentoAula != null)
        {
            camaraFragmentoAula.gameObject.SetActive(true);
            camaraFragmentoAula.Priority = 40;

            if (camaraCinematicaAula != null)
                camaraCinematicaAula.gameObject.SetActive(false);

            yield return new WaitForSeconds(duracionPlanoFragmento);

            camaraFragmentoAula.gameObject.SetActive(false);
        }

        // Volver a Dani.
        if (camaraDani != null)
            camaraDani.Priority = 30;

        if (camaraCinematicaAula != null)
            camaraCinematicaAula.gameObject.SetActive(false);

        if (fantasmaAula != null)
            fantasmaAula.SetActive(true);

        yield return new WaitForSeconds(esperaRegresoCamara);

        if (gestorEntradas != null)
            gestorEntradas.enabled = true;

        if (movimientoDani != null)
            movimientoDani.Bloquear(false);

        Debug.Log(
            "Cinematica inicial del aula finalizada."
        );
    }

    // Se llama al recoger la llave de biblioteca.
    public void RegistrarFragmento()
    {
        if (!eventoIniciado || puzzleCompletado)
            return;

        puzzleCompletado = true;

        StartCoroutine(SecuenciaLiberacion());
    }

    private IEnumerator SecuenciaLiberacion()
    {
        Debug.Log("Iniciando final del puzzle del aula.");

        cinemáticaFinalActiva = true;

        // 1. Bloquear a Dani durante la cinematica.
        if (interaccionDani != null)
        {
            interaccionDani.EstablecerBloqueoCinematica(true);
        }
        else if (movimientoDani != null)
        {
            movimientoDani.Bloquear(true);
        }

        // Esperar a que termine la animacion de recoger.
        yield return new WaitForSeconds(0.8f);

        // 2. Enfocar los objetos flotantes.
        if (camaraNegatividad != null)
        {
            camaraNegatividad.gameObject.SetActive(true);
            camaraNegatividad.Priority = 50;
        }

        yield return new WaitForSeconds(
            tiempoTransicionNegatividad
        );

        // 3. Mostrar la negatividad antes de desaparecer.
        yield return new WaitForSeconds(
            tiempoObservandoNegatividad
        );

        // 4. Iniciar desaparicion gradual.
        if (objetosNegatividad != null)
        {
            objetosNegatividad.IniciarDesaparicion();
        }

        // Desactivar el fantasma.
        if (fantasmaAula != null)
            fantasmaAula.SetActive(false);

        // Esperar a que termine el efecto visual.
        yield return new WaitForSeconds(
            tiempoDesaparicionNegatividad
        );

        // 5. Dani recupera su tamaño original.
        if (efectoEncogimiento != null)
            efectoEncogimiento.RecuperarTamanoPorPuzzle();

        // 6. Registrar progreso.
        if (!string.IsNullOrEmpty(idPuzzle) &&
            GameProgressManager.Instancia != null)
        {
            GameProgressManager.Instancia
                .RegistrarPuzzleCompletado(idPuzzle);
        }

        // Esperar recuperacion del tamaño.
        float tiempo = 0f;

        while (
            efectoEncogimiento != null &&
            efectoEncogimiento.ObtenerFactorActual() < 0.99f &&
            tiempo < 5f
        )
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(retrasoApertura);

        // 7. Abrir la puerta del aula.
        if (puertaAula != null)
            puertaAula.AbrirAlCompletarPuzzle();

        // 8. Regresar a Dani.
        if (camaraDani != null)
            camaraDani.Priority = 60;

        yield return new WaitForSeconds(esperaRegresoCamara);

        // Restaurar prioridades.
        if (camaraDani != null)
            camaraDani.Priority = prioridadOriginalDani;

        if (camaraNegatividad != null)
        {
            camaraNegatividad.Priority =
                prioridadOriginalNegatividad;

            camaraNegatividad.gameObject.SetActive(false);
        }

        // 9. Devolver controles.
        if (interaccionDani != null)
        {
            interaccionDani.EstablecerBloqueoCinematica(false);
        }
        else if (movimientoDani != null)
        {
            movimientoDani.Bloquear(false);
        }

        cinemáticaFinalActiva = false;

        Debug.Log(
            "Puzzle del aula completado. Dani puede salir."
        );
    }
}
