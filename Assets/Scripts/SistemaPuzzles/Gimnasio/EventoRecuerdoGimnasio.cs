
using System.Collections;
using UnityEngine;
using TMPro;
using Cinemachine;

public class EventoRecuerdoGimnasio : MonoBehaviour
{
    [Header("Recuerdo")]
    [SerializeField] private GameObject grupoSiluetas;
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TMP_Text textoDialogo;

    [TextArea(2, 4)]
    [SerializeField]
    private string frase =
        "Ya estamos completos. No hay lugar para vos.";

    [Header("Cámaras Cinemachine")]
    [SerializeField] private CinemachineVirtualCameraBase camaraDani;
    [SerializeField] private CinemachineVirtualCameraBase camaraRecuerdo;
    [SerializeField] private CinemachineVirtualCameraBase camaraNiebla;

    [Header("Movimiento")]
    [SerializeField] private MovimientoPersonaje movimientoDani;

    [Header("Diálogo final de Dani")]
    [SerializeField] private DialogoInicial sistemaDialogo;

    [SerializeField, TextArea]
    private string fraseDani = "¿Por qué...?";

    [SerializeField] private float duracionDialogoDani = 3f;

    [Header("Panel del puzzle")]
    [SerializeField] private TableroPuzzleGimnasio tableroPuzzle;

    [Header("Duraciones")]
    [SerializeField] private float esperaInicial = 1f;
    [SerializeField] private float duracionRecuerdo = 6f;
    [SerializeField] private float duracionVistaNiebla = 3f;
    [SerializeField] private float esperaRegreso = 1f;

    [Header("Desbloqueo")]
    [SerializeField] private GameObject barreraNiebla;

    private bool recuerdoActivado = false;

    private int prioridadOriginalDani;
    private int prioridadOriginalRecuerdo;
    private int prioridadOriginalNiebla;

    private void Start()
    {
        if (grupoSiluetas != null)
            grupoSiluetas.SetActive(false);

        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        prioridadOriginalDani = camaraDani.Priority;
        prioridadOriginalRecuerdo = camaraRecuerdo.Priority;
        prioridadOriginalNiebla = camaraNiebla.Priority;
    }

    public void ActivarRecuerdo()
    {
        if (recuerdoActivado)
            return;

        recuerdoActivado = true;
        StartCoroutine(SecuenciaRecuerdo());
    }

    private IEnumerator SecuenciaRecuerdo()
    {
        Debug.Log("Comienza el recuerdo del gimnasio.");

        // 1. Cerrar el panel del puzzle.
        if (tableroPuzzle != null)
            tableroPuzzle.CerrarPanel();

        // 2. Bloquear el movimiento de Dani.
        if (movimientoDani != null)
            movimientoDani.Bloquear(true);

        yield return new WaitForSeconds(esperaInicial);

        // 3. Enfocar las siluetas.
        camaraRecuerdo.Priority = 20;

        yield return new WaitForSeconds(1f);

        // 4. Mostrar el recuerdo.
        if (grupoSiluetas != null)
            grupoSiluetas.SetActive(true);

        if (textoDialogo != null)
            textoDialogo.text = frase;

        if (panelDialogo != null)
            panelDialogo.SetActive(true);

        yield return new WaitForSeconds(duracionRecuerdo);

        // 5. Ocultar las siluetas y el diálogo.
        if (grupoSiluetas != null)
            grupoSiluetas.SetActive(false);

        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        // 6. Enfocar la barrera de niebla.
        camaraNiebla.Priority = 30;

        yield return new WaitForSeconds(duracionVistaNiebla);

        // 7. Desbloquear el vestuario.
        if (barreraNiebla != null)
            barreraNiebla.SetActive(false);

        yield return new WaitForSeconds(1f);

        // 8. Regresar a la cámara de Dani.
        camaraDani.Priority = 40;

        // Esperar a que termine la transición de cámara.
        yield return new WaitForSeconds(esperaRegreso);

        // 9. Restaurar las prioridades originales.
        camaraDani.Priority = prioridadOriginalDani;
        camaraRecuerdo.Priority = prioridadOriginalRecuerdo;
        camaraNiebla.Priority = prioridadOriginalNiebla;

        // 10. Pequeña pausa dramática.
        yield return new WaitForSeconds(0.5f);

        // 11. Dani reacciona al recuerdo.
        if (sistemaDialogo != null)
        {
            sistemaDialogo.MostrarDialogo(
                fraseDani,
                duracionDialogoDani
            );
        }

        // Mantener a Dani inmóvil mientras habla.
        yield return new WaitForSeconds(duracionDialogoDani);

        // 12. Devolver el control a Dani.
        if (movimientoDani != null)
            movimientoDani.Bloquear(false);

        Debug.Log("Recuerdo finalizado. Vestuario desbloqueado.");
    }
}
