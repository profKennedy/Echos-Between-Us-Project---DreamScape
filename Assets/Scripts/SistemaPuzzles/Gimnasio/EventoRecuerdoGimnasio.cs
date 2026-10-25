using System.Collections;
using UnityEngine;
using TMPro;

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

    [Header("Duracion")]
    [SerializeField] private float duracionRecuerdo = 15f;

    [Header("Desbloqueo")]
    [SerializeField] private GameObject barreraNiebla;

    private bool recuerdoActivado = false;

    private void Start()
    {
        if (grupoSiluetas != null)
            grupoSiluetas.SetActive(false);

        if (panelDialogo != null)
            panelDialogo.SetActive(false);
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

        // Aparecen las siluetas.
        if (grupoSiluetas != null)
            grupoSiluetas.SetActive(true);

        // Aparece el dialogo.
        if (textoDialogo != null)
            textoDialogo.text = frase;

        if (panelDialogo != null)
            panelDialogo.SetActive(true);

        // El recuerdo permanece durante 15 segundos.
        yield return new WaitForSeconds(duracionRecuerdo);

        // Desaparecen las siluetas.
        if (grupoSiluetas != null)
            grupoSiluetas.SetActive(false);

        // Desaparece el dialogo.
        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        // Se desbloquea el vestuario.
        if (barreraNiebla != null)
            barreraNiebla.SetActive(false);

        Debug.Log("Recuerdo finalizado. Vestuario desbloqueado.");
    }
}