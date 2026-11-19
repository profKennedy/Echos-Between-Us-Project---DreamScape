
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

    [Header("Configuración del diálogo inicial")]
    [SerializeField]
    private string mensaje =
        "Donde estoy... ¿Mi escuela?";

    [SerializeField] private MovimientoPersonaje movimientoDany;
    [SerializeField] private float duracion = 4f;

    [Header("Tutorial de controles")]
    [SerializeField] private TutorialControles tutorialControles;

    [Header("Espera antes del tutorial")]
    [SerializeField] private float esperaTutorial = 3f;


    private Coroutine rutinaDialogo;
    private bool secuenciaInicialActiva;

    private void Start()
    {
        StartCoroutine(SecuenciaInicial());
    }

    private IEnumerator SecuenciaInicial()
    {
        secuenciaInicialActiva = true;

        movimientoDany.Bloquear(true);
        panelDialogo.SetActive(false);

        // Comenzar con pantalla negra.
        Color color = imagenFadeInicio.color;
        color.a = 1f;
        imagenFadeInicio.color = color;

        imagenFadeInicio.gameObject.SetActive(true);

        // Fade In inicial.
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

        color.a = 0f;
        imagenFadeInicio.color = color;
        imagenFadeInicio.gameObject.SetActive(false);

        // Mostrar diálogo inicial.
        textoDialogo.text = mensaje;
        panelDialogo.SetActive(true);

        yield return new WaitForSeconds(duracion);

        // Ocultar el diálogo inicial.
        panelDialogo.SetActive(false);

        // Devolver el control a Dani.
        movimientoDany.Bloquear(false);
        secuenciaInicialActiva = false;

        // Esperar 3 segundos antes de mostrar el tutorial.
        yield return new WaitForSeconds(esperaTutorial);

        // Mostrar el tutorial correspondiente a la plataforma.
        if (tutorialControles != null)
        {
            tutorialControles.MostrarTutorial();
        }
    }

    // Permite mostrar nuevos diálogos
    // reutilizando el mismo panel.
    public void MostrarDialogo(string nuevoMensaje, float tiempo)
    {
        // No interrumpir la presentación inicial.
        if (secuenciaInicialActiva)
            return;

        // Evitar que se superpongan diálogos.
        if (rutinaDialogo != null)
            StopCoroutine(rutinaDialogo);

        rutinaDialogo = StartCoroutine(
            RutinaMostrarDialogo(nuevoMensaje, tiempo)
        );
    }

    private IEnumerator RutinaMostrarDialogo(
        string nuevoMensaje,
        float tiempo)
    {
        textoDialogo.text = nuevoMensaje;
        panelDialogo.SetActive(true);

        yield return new WaitForSeconds(tiempo);

        panelDialogo.SetActive(false);
        rutinaDialogo = null;
    }
}
