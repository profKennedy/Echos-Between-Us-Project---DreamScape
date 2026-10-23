using UnityEngine;
using UnityEngine.UI;

public class PasaPaginasUI : MonoBehaviour
{
    [Header("Conexión con el otro script")]
    public ControladorFade efectoFade; // Referencia al script visual

    [Header("Imágenes y Texturas")]
    public Image imagenIzquierda;
    public Image imagenDerecha;   
    public Sprite[] todasLasPaginas; 
    
    private int paginaActual = 0;
    private bool cambiando = false; 

    void Start()
    {
        ActualizarSprites();
    }

    public void AvanzarPagina()
    {
        if (paginaActual + 2 < todasLasPaginas.Length && !cambiando)
        {
            paginaActual += 2;
            EjecutarCambio();
        }
    }

    public void RetrocederPagina()
    {
        if (paginaActual >= 2 && !cambiando)
        {
            paginaActual -= 2;
            EjecutarCambio();
        }
    }

    private void EjecutarCambio()
    {
        cambiando = true; // Bloquea los botones temporamente
        
        // Llama al otro script y le pasa la función ActualizarSprites como una tarea "=>"
        efectoFade.IniciarFade(() => 
        {
            ActualizarSprites();
        });

        // Desbloquea los botones sumando el tiempo de ida y vuelta del fade + un margen de seguridad
        Invoke("DesbloquearBotones", efectoFade.duracionFade * 2.1f);
    }

    private void ActualizarSprites()
    {
        if (todasLasPaginas.Length == 0) return;

        if (paginaActual < todasLasPaginas.Length)
        {
            imagenIzquierda.sprite = todasLasPaginas[paginaActual];
            imagenIzquierda.enabled = true;
        }
        else imagenIzquierda.enabled = false;
        
        if ((paginaActual + 1) < todasLasPaginas.Length)
        {
            imagenDerecha.sprite = todasLasPaginas[paginaActual + 1];
            imagenDerecha.enabled = true;
        }
        else imagenDerecha.enabled = false;
    }

    private void DesbloquearBotones()
    {
        cambiando = false;
    }
}