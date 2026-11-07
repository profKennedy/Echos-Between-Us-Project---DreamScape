
using System.Collections;
using UnityEngine;
using Cinemachine;

public class EventoEncogimientoAula : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private EfectoEncogimiento efectoEncogimiento;
    [SerializeField] private MovimientoPersonaje movimientoDani;
    [SerializeField] private GestorEntradas gestorEntradas;

    [Header("Configuracion")]
    [SerializeField] private float tiempoMaximo = 5f;

    [Header("Puerta del aula")]
    [SerializeField] private Puerta puertaAula;
    [SerializeField] private float retrasoCierre = 0.5f;

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

        // 6. Regresar a la cámara normal.
        if (camaraDani != null)
            camaraDani.Priority = 30;

        if (camaraCinematicaAula != null)
            camaraCinematicaAula.gameObject.SetActive(false);

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
