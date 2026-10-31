using System.Collections;
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

    [Header("Transición al juego")]
    [SerializeField] private string nombreEscenaJuego = "EscenaVerticalSlice";
    [SerializeField] private float tiempoEsperaFinal = 10f;

    private int paginaActual = 0;
    private bool cambiando = false;

    // Cuando llega a la última página, bloqueamos el libro.
    private bool libroFinalizado = false;

    private void Start()
    {
        ActualizarSprites();
        VerificarUltimaPagina();
    }

    public void AvanzarPagina()
    {
        // No permitir avanzar si el libro terminó.
        if (cambiando || libroFinalizado) return;

        if (paginaActual + 2 < todasLasPaginas.Length)
        {
            paginaActual += 2;
            EjecutarCambio();
        }
    }

    public void RetrocederPagina()
    {
        // No permitir retroceder si el libro terminó.
        if (cambiando || libroFinalizado) return;

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

        // Esperamos a que termine el Fade.
        Invoke(
            nameof(DesbloquearBotones),
            efectoFade.duracionFade * 2.1f
        );
    }

    private void ActualizarSprites()
    {
        if (todasLasPaginas.Length == 0) return;

        // Página izquierda.
        if (paginaActual < todasLasPaginas.Length)
        {
            imagenIzquierda.sprite = todasLasPaginas[paginaActual];
            imagenIzquierda.enabled = true;
        }
        else
        {
            imagenIzquierda.enabled = false;
        }

        // Página derecha.
        if (paginaActual + 1 < todasLasPaginas.Length)
        {
            imagenDerecha.sprite = todasLasPaginas[paginaActual + 1];
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

        VerificarUltimaPagina();
    }

    private void VerificarUltimaPagina()
    {
        if (libroFinalizado || todasLasPaginas.Length == 0)
            return;

        // Comprobar si estamos en el último par de páginas.
        bool esUltimaPagina =
            paginaActual + 2 >= todasLasPaginas.Length;

        if (esUltimaPagina)
        {
            // Bloqueamos definitivamente la navegación.
            libroFinalizado = true;

            Debug.Log("Última página alcanzada. Libro bloqueado.");

            StartCoroutine(EsperarYCargarJuego());
        }
    }

    private IEnumerator EsperarYCargarJuego()
    {
        Debug.Log("Esperando 10 segundos antes de iniciar el juego...");

        yield return new WaitForSeconds(tiempoEsperaFinal);

        Debug.Log("Cargando escena del juego...");

        SceneManager.LoadScene(nombreEscenaJuego);
    }
}