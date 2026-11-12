using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

// Recorre las cámaras de los libros, una por una. En cada plano espera a que Cinemachine
// termine de mezclar hacia la cámara y recién ahí empieza el tiempo del plano, con un
// movimiento leve (acercamiento + deriva lateral) para que no quede completamente estático.
public class PresentacionLibros : MonoBehaviour
{
    [Serializable]
    public class PlanoLibro
    {
        public CinemachineVirtualCamera camara;
    }

    [SerializeField] private PlanoLibro[] planos;

    [Header("Cada plano (valores iguales para los 3)")]
    [Tooltip("Segundos que se queda mirando el libro DESPUÉS de que terminó el blend de Cinemachine.")]
    [SerializeField] private float duracionPlano = 2.5f;

    [Tooltip("Metros que la cámara avanza hacia adelante durante el plano.")]
    [SerializeField] private float empuje = 0.8f;

    [Tooltip("Metros que se corre hacia un costado. Alterna izquierda/derecha entre planos.")]
    [SerializeField] private float deriva = 0.25f;

    [Header("Cinemachine")]
    [Tooltip("Tiene que ser mayor que la Priority de la cámara de Dani (y de cualquier otra cámara que pueda estar activa).")]
    [SerializeField] private int prioridadPlano = 50;

    [Tooltip("Solo se usa si no hay CinemachineBrain en la escena: espera fija al volver a la cámara de Dani.")]
    [SerializeField] private float esperaRegreso = 1f;

    private CinemachineBrain _brain;

    private void Awake()
    {
        _brain = FindFirstObjectByType<CinemachineBrain>();

        // Las cámaras de los libros no pueden estar activas antes de la cinemática,
        // si no una de ellas le gana a la cámara de Dani por Priority.
        foreach (PlanoLibro plano in planos)
            if (plano.camara != null) plano.camara.gameObject.SetActive(false);
    }

    public IEnumerator Reproducir()
    {
        CinemachineVirtualCamera anterior = null;

        // Posiciones originales, para dejar las cámaras como estaban al terminar
        Vector3[] posicionesOriginales = new Vector3[planos.Length];
        for (int i = 0; i < planos.Length; i++)
            if (planos[i].camara != null) posicionesOriginales[i] = planos[i].camara.transform.position;

        for (int i = 0; i < planos.Length; i++)
        {
            CinemachineVirtualCamera camara = planos[i].camara;
            if (camara == null) continue;

            Transform t = camara.transform;
            float sentido = (i % 2 == 0) ? 1f : -1f;
            Vector3 posicionInicial = t.position;
            Vector3 destino = posicionInicial + t.forward * empuje + t.right * (deriva * sentido);

            // La anterior queda activa pero con Priority baja: así Cinemachine mezcla
            // desde ella hacia la nueva en lugar de saltar.
            camara.Priority = prioridadPlano;
            camara.gameObject.SetActive(true);
            if (anterior != null) anterior.Priority = 0;

            // Primero llegamos a la cámara; el tiempo del plano empieza cuando el blend terminó
            yield return EsperarBlend();

            float duracion = Mathf.Max(0.1f, duracionPlano);
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, tiempo / duracion);
                t.position = Vector3.Lerp(posicionInicial, destino, k);
                yield return null;
            }

            anterior = camara;
        }

        // Soltamos la última y esperamos a que Cinemachine vuelva a Dani.
        if (anterior != null) anterior.Priority = 0;
        yield return EsperarBlend();

        // Dejamos todas las cámaras apagadas y en su posición original
        for (int i = 0; i < planos.Length; i++)
        {
            CinemachineVirtualCamera camara = planos[i].camara;
            if (camara == null) continue;
            camara.gameObject.SetActive(false);
            camara.transform.position = posicionesOriginales[i];
        }
    }

    // Espera a que Cinemachine termine de mezclar entre cámaras
    private IEnumerator EsperarBlend()
    {
        if (_brain == null)
        {
            yield return new WaitForSeconds(esperaRegreso);
            yield break;
        }

        // El Brain registra el cambio de cámara en su LateUpdate: hay que dejar pasar un frame
        yield return null;

        float limite = 6f; // por si algo se traba, nunca esperamos más que esto
        while (_brain.IsBlending && limite > 0f)
        {
            limite -= Time.deltaTime;
            yield return null;
        }
    }
}