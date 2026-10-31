using System.Collections;
using UnityEngine;
using TMPro;

public class DialogoInicial : MonoBehaviour
{
    [Header("Interfaz")]
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TMP_Text textoDialogo;

    [Header("Configuracion")]
    [SerializeField]
    private string mensaje =
        "Donde estoy... Mi escuela?";

    [SerializeField] private float duracion = 4f;

    private void Start()
    {
        StartCoroutine(MostrarDialogo());
    }

    private IEnumerator MostrarDialogo()
    {
        // Mostrar el diálogo.
        textoDialogo.text = mensaje;
        panelDialogo.SetActive(true);

        // Esperar unos segundos.
        yield return new WaitForSeconds(duracion);

        // Ocultar el diálogo.
        panelDialogo.SetActive(false);
    }
}