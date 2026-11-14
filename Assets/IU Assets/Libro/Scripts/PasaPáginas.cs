
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PasaPaginasUI : MonoBehaviour
{
    [Header("Conexión con el otro script")]
    public ControladorFade efectoFade;

    [Header("Imágenes y Texturas")]
    public Image imagenIzquierda;
    public Image imagenDerecha;
    public Sprite[] todasLasPaginas;

    [Header("Transición a la cinemática")]
    [SerializeField] private string nombreEscenaJuego = "EscenaVerticalSlice";

    private int paginaActual = 0;
    private bool cambiando = false;
    private bool libroFinalizado = false;

    private void Start()
    {
        ActualizarSprites();
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
            // Una nueva pulsación inicia la cinemática.
            libroFinalizado = true;
            cambiando = true;

            Debug.Log("Libro terminado. Cargando cinemática...");

            SceneManager.LoadScene(nombreEscenaJuego);
        }
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
