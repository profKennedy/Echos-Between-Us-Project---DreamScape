
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PasaPaginasUI : MonoBehaviour
{
    [Header("Conexión con el otro script")]
    public ControladorFade efectoFade;

    [Header("Animación de apertura del libro")]
    public Animator animatorLibro;
    public float esperaFadeInicial = 1.7f;      // Tiempo que espera a que termine FadeAlEntrar
    public float duracionAnimApertura = 1.5f;   // Tiempo que tarda el libro en abrirse antes de habilitar botones

    [Header("Imágenes y Texturas")]
    public Image imagenIzquierda;
    public Image imagenDerecha;
    public Sprite[] todasLasPaginas;

    [Header("Transición a la cinemática")]
    [SerializeField] private string nombreEscenaJuego = "EscenaVerticalSlice";
    public Image faderEscena;                   // Arrastrá acá tu ImageFader
    public float duracionFadeSalida = 1.5f;     // Duración del fade a negro al salir

    private int paginaActual = 0;
    private bool cambiando = true; // Arranca bloqueado hasta que el libro se abra
    private bool libroFinalizado = false;

    private void Awake()
    {
        // Clava el libro cerrado en el primer frame antes de que arranque el fade
        if (animatorLibro != null)
        {
            animatorLibro.Update(0f);
            animatorLibro.speed = 0f;
        }
    }

    private void Start()
    {
        ActualizarSprites();
        StartCoroutine(SecuenciaApertura());
    }

    private IEnumerator SecuenciaApertura()
    {
        // 1. Espera a que termine el fade de entrada de la escena
        yield return new WaitForSecondsRealtime(esperaFadeInicial);

        // 2. Suelta la animación para que el libro se abra
        if (animatorLibro != null)
        {
            animatorLibro.speed = 1f;
        }

        // 3. Espera a que termine de abrirse para habilitar pasar/volver página
        if (duracionAnimApertura > 0f)
        {
            yield return new WaitForSecondsRealtime(duracionAnimApertura);
        }

        cambiando = false;
    }

    public void AvanzarPagina()
    {
        if (cambiando || libroFinalizado)
            return;

        // Si todavía quedan páginas, avanzamos normalmente.
        if (paginaActual + 2 < todasLasPaginas.Length)
        {
            paginaActual += 2;
            EjecutarCambio();
        }
        else
        {
            // Ya estamos en las últimas páginas.
            // Una nueva pulsación hace el fade a negro y luego carga la cinemática.
            libroFinalizado = true;
            cambiando = true;

            Debug.Log("Libro terminado. Haciendo fade y cargando cinemática...");
            StartCoroutine(FadeSalidaYCargar());
        }
    }

    private IEnumerator FadeSalidaYCargar()
    {
        if (faderEscena != null)
        {
            faderEscena.gameObject.SetActive(true);
            faderEscena.raycastTarget = true;
            Color color = faderEscena.color;
            float alphaInicial = color.a;
            float tiempo = 0f;

            while (tiempo < duracionFadeSalida)
            {
                tiempo += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                float progreso = Mathf.Clamp01(tiempo / duracionFadeSalida);
                color.a = Mathf.SmoothStep(alphaInicial, 1f, progreso);
                faderEscena.color = color;
                yield return null;
            }

            color.a = 1f;
            faderEscena.color = color;
        }

        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void RetrocederPagina()
    {
        if (cambiando || libroFinalizado)
            return;

        if (paginaActual >= 2)
        {
            paginaActual -= 2;
            EjecutarCambio();
        }
    }

    private void EjecutarCambio()
    {
        cambiando = true;

        efectoFade.IniciarFade(() =>
        {
            ActualizarSprites();
        });

        Invoke(
            nameof(DesbloquearBotones),
            efectoFade.duracionFade * 2.1f
        );
    }

    private void ActualizarSprites()
    {
        if (todasLasPaginas == null ||
            todasLasPaginas.Length == 0)
            return;

        if (paginaActual < todasLasPaginas.Length)
        {
            imagenIzquierda.sprite =
                todasLasPaginas[paginaActual];

            imagenIzquierda.enabled = true;
        }
        else
        {
            imagenIzquierda.enabled = false;
        }

        if (paginaActual + 1 < todasLasPaginas.Length)
        {
            imagenDerecha.sprite =
                todasLasPaginas[paginaActual + 1];

            imagenDerecha.enabled = true;
        }
        else
        {
            imagenDerecha.enabled = false;
        }
    }

    private void DesbloquearBotones()
    {
        cambiando = false;
    }
}
