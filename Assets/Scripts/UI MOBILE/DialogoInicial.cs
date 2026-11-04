using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogoInicial : MonoBehaviour
{
    [Header("Interfaz del diálogo")]
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TMP_Text textoDialogo;

    [Header("Fade In")]
    [SerializeField] private Image imagenFadeInicio;
    [SerializeField] private float duracionFade = 2f;

    [Header("Configuración del diálogo")]
    [SerializeField]
    private string mensaje =
        "Donde estoy... ¿Mi escuela?";

    [SerializeField] private MovimientoPersonaje movimientoDany;

    [SerializeField] private float duracion = 4f;

    private void Start()
    {
        StartCoroutine(SecuenciaInicial());
    }

    private IEnumerator SecuenciaInicial()
    {
        movimientoDany.Bloquear(true);
        // Ocultar el diálogo al comenzar.
        panelDialogo.SetActive(false);

        // Comenzar con la pantalla completamente negra.
        Color color = imagenFadeInicio.color;
        color.a = 1f;
        imagenFadeInicio.color = color;

        // Fade In: revelar progresivamente la escuela.
        float tiempo = 0f;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;

            color.a = Mathf.Lerp(
                1f,
                0f,
                Mathf.Clamp01(tiempo / duracionFade)
            );

            imagenFadeInicio.color = color;

            yield return null;
        }

        // Asegurar transparencia total.
        color.a = 0f;
        imagenFadeInicio.color = color;

        // Ocultar la imagen del Fade.
        imagenFadeInicio.gameObject.SetActive(false);

        // Mostrar el diálogo de Dany.
        textoDialogo.text = mensaje;
        panelDialogo.SetActive(true);

        // Mantener el diálogo visible.
        yield return new WaitForSeconds(duracion);

        // Ocultar el diálogo.
        panelDialogo.SetActive(false);

        movimientoDany.Bloquear(false);
    }
}