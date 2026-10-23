using System.Collections;
using System; // Necesario para Action
using UnityEngine;
using UnityEngine.UI;

public class ControladorFade : MonoBehaviour
{
    [Header("Imágenes a hacer Fade")]
    public Image imagenIzquierda;
    public Image imagenDerecha;
    
    [Header("Configuración")]
    public float duracionFade = 0.2f;

    // Recibe una "tarea" (Action) para ejecutar cuando las imágenes estén invisibles
    public void IniciarFade(Action tareaAlEstarInvisible)
    {
        StartCoroutine(RutinaFade(tareaAlEstarInvisible));
    }

    private IEnumerator RutinaFade(Action tareaAlEstarInvisible)
    {
        float tiempo = 0f;
        Color colorIzq = imagenIzquierda.color;
        Color colorDer = imagenDerecha.color;

        // 1. Fade Out (Desaparecer)
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempo / duracionFade);
            
            colorIzq.a = alpha; colorDer.a = alpha;
            if (imagenIzquierda.enabled) imagenIzquierda.color = colorIzq;
            if (imagenDerecha.enabled) imagenDerecha.color = colorDer;
            
            yield return null;
        }

        // 2. Ejecutar la tarea (Cambiar los Sprites en el otro script)
        if (tareaAlEstarInvisible != null)
        {
            tareaAlEstarInvisible.Invoke();
        }

        // 3. Fade In (Aparecer)
        tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tiempo / duracionFade);
            
            colorIzq.a = alpha; colorDer.a = alpha;
            if (imagenIzquierda.enabled) imagenIzquierda.color = colorIzq;
            if (imagenDerecha.enabled) imagenDerecha.color = colorDer;
            
            yield return null;
        }
        
        // Asegurar alpha al 100% al terminar
        colorIzq.a = 1f; colorDer.a = 1f;
        if (imagenIzquierda.enabled) imagenIzquierda.color = colorIzq;
        if (imagenDerecha.enabled) imagenDerecha.color = colorDer;
    }
}