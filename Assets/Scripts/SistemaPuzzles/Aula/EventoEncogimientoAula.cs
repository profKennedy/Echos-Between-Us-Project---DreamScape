
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
}
