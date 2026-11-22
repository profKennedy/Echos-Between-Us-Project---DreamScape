
using System.Collections;
using UnityEngine;
using Cinemachine;
using TMPro;

public class EventoRecuerdoBano : MonoBehaviour
{
    [Header("Sistema de interacción")]
    [SerializeField] private InteraccionPersonaje interaccionDani;

    [Header("Jugador")]
    [SerializeField] private MovimientoPersonaje movimientoDani;

    [Header("Recuerdo")]
    [SerializeField] private GameObject siluetaDani;
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TMP_Text textoDialogo;

    [TextArea(2, 4)]
    [SerializeField]
    private string frase =
        "No quiero que me vean llorar...";

    [Header("Color del diálogo")]
    [SerializeField]
    private Color colorDialogoRecuerdo =
    new Color(0.55f, 0.32f, 0.18f, 1f);

    private Color colorOriginalTexto;

    [Header("Audio")]
    [SerializeField] private AudioSource audioRecuerdo;
    [SerializeField] private AudioClip vozDani;

    [Header("Elementos del escenario")]
    [SerializeField] private GameObject muroNiebla;
    [SerializeField] private GameObject fantasmaPasillo;

    [Header("Progreso del recuerdo")]
    [SerializeField] private string idProgresoRecuerdo = "RecuerdoBano";
    [SerializeField] private bool registrarProgresoAlFinal = true;

    [Header("Cámaras Cinemachine")]
    [SerializeField] private CinemachineVirtualCameraBase camaraDani;
    [SerializeField] private CinemachineVirtualCameraBase camaraMuroNiebla;

    [Header("Duración del recuerdo")]
    [SerializeField] private float esperaInicial = 1f;
    [SerializeField] private float duracionRecuerdo = 5f;

    [Header("Tiempos de cámara")]
    [SerializeField] private float tiempoTransicionMuro = 1.5f;
    [SerializeField] private float tiempoObservandoMuro = 2f;
    [SerializeField] private float tiempoMuroDesbloqueado = 2f;
    [SerializeField] private float esperaRegresoCamara = 1.5f;

    private int prioridadOriginalDani;
    private int prioridadOriginalMuro;

    private bool activado = false;

    private void Start()
    {
        // La silueta comienza oculta.
        if (siluetaDani != null)
            siluetaDani.SetActive(false);

        // El diálogo comienza oculto.
        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        // Guardar las prioridades originales.
        if (camaraDani != null)
            prioridadOriginalDani = camaraDani.Priority;

        if (camaraMuroNiebla != null)
            prioridadOriginalMuro = camaraMuroNiebla.Priority;

        // Guardar el color original del texto.
        if (textoDialogo != null)
        {
            colorOriginalTexto = textoDialogo.color;
        }
    }

    public void ActivarRecuerdo()
    {
        if (activado)
            return;

        activado = true;

        StartCoroutine(SecuenciaRecuerdo());
    }

    private IEnumerator SecuenciaRecuerdo()
    {
        Debug.Log("Iniciando recuerdo del baño.");

        // -------------------------------------------------
        // 1. BLOQUEAR A DANI DURANTE LA CINEMÁTICA
        // -------------------------------------------------

        if (interaccionDani != null)
        {
            interaccionDani.EstablecerBloqueoCinematica(true);

            Debug.Log(
                "Dani bloqueada durante la cinemática."
            );
        }
        else if (movimientoDani != null)
        {
            movimientoDani.Bloquear(true);

            Debug.LogWarning(
                "InteraccionPersonaje no asignado. " +
                "Se utiliza el bloqueo de movimiento."
            );
        }
        else
        {
            Debug.LogError(
                "No se asignaron las referencias de Dani."
            );
        }

        yield return new WaitForSeconds(esperaInicial);

        // -------------------------------------------------
        // 2. MOSTRAR EL RECUERDO DE DANI
        // -------------------------------------------------

        if (siluetaDani != null)
            siluetaDani.SetActive(true);

        if (textoDialogo != null)
        {
            textoDialogo.text = frase;
            textoDialogo.color = colorDialogoRecuerdo;
        }

        if (panelDialogo != null)
            panelDialogo.SetActive(true);

        if (audioRecuerdo != null && vozDani != null)
            audioRecuerdo.PlayOneShot(vozDani);

        Debug.Log("Mostrando recuerdo de Dani.");

        yield return new WaitForSeconds(duracionRecuerdo);

        // -------------------------------------------------
        // 3. OCULTAR EL RECUERDO
        // -------------------------------------------------

        // Ocultar el recuerdo.
        if (siluetaDani != null)
            siluetaDani.SetActive(false);

        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        // Restaurar el color original del diálogo.
        if (textoDialogo != null)
        {
            textoDialogo.color = colorOriginalTexto;
        }

        // -------------------------------------------------
        // 4. CAMBIAR A LA CÁMARA DEL MURO
        // -------------------------------------------------

        if (camaraMuroNiebla != null)
        {
            camaraMuroNiebla.Priority = 30;
        }

        Debug.Log("Cámara enfocando el muro de niebla.");

        // Esperar la transición de Cinemachine.
        yield return new WaitForSeconds(tiempoTransicionMuro);

        // Mantener el muro visible unos segundos.
        yield return new WaitForSeconds(tiempoObservandoMuro);

        // -------------------------------------------------
        // 5. DESBLOQUEAR EL PASILLO CENTRAL
        // -------------------------------------------------

        // Registrar el progreso recién ahora.
        if (registrarProgresoAlFinal)
        {
            if (GameProgressManager.Instancia != null)
            {
                GameProgressManager.Instancia
                    .RegistrarPuzzleCompletado(
                        idProgresoRecuerdo
                    );
            }
            else
            {
                Debug.LogError(
                    "No existe GameProgressManager."
                );
            }
        }

        // Desactivar el muro de niebla.
        if (muroNiebla != null)
        {
            muroNiebla.SetActive(false);
            Debug.Log("Muro de niebla desactivado.");
        }

        // Desactivar el fantasma del pasillo.
        if (fantasmaPasillo != null)
        {
            fantasmaPasillo.SetActive(false);
            Debug.Log("Fantasma del pasillo desactivado.");
        }

        // Mostrar el camino ya desbloqueado.
        yield return new WaitForSeconds(
            tiempoMuroDesbloqueado
        );

        // -------------------------------------------------
        // 6. REGRESAR A LA CÁMARA DE DANI
        // -------------------------------------------------

        if (camaraDani != null)
        {
            camaraDani.Priority = 40;
        }

        yield return new WaitForSeconds(
            esperaRegresoCamara
        );

        // Restaurar prioridades originales.
        if (camaraDani != null)
            camaraDani.Priority = prioridadOriginalDani;

        if (camaraMuroNiebla != null)
            camaraMuroNiebla.Priority = prioridadOriginalMuro;

        // -------------------------------------------------
        // 7. DEVOLVER EL CONTROL A DANI
        // -------------------------------------------------

        if (interaccionDani != null)
        {
            interaccionDani.EstablecerBloqueoCinematica(false);
        }
        else if (movimientoDani != null)
        {
            movimientoDani.Bloquear(false);
        }

        Debug.Log(
            "Recuerdo del baño completado. " +
            "Dani recupera el movimiento."
        );
    }
}
