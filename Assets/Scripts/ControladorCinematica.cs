using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControladorCinematica : MonoBehaviour
{
    [Header("Reproductor")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Transición")]
    [SerializeField] private Image imagenFadeNegro;
    [SerializeField] private float duracionFadeEntrada = 1.5f;
    [SerializeField] private float duracionFade = 2f;

    [Header("Escena siguiente")]
    [SerializeField] private string escenaSiguiente = "EscenaVerticalSlice";

    private bool cambiandoEscena = false;
    private bool fadeIniciado = false;
    private Coroutine corrutinaFadeActual;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Arranca completamente en negro antes del primer frame
        if (imagenFadeNegro != null)
        {
            imagenFadeNegro.gameObject.SetActive(true);
            Color color = imagenFadeNegro.color;
            color.a = 1f;
            imagenFadeNegro.color = color;
        }
    }

    private void Start()
    {
        if (videoPlayer == null || imagenFadeNegro == null)
        {
            Debug.LogError("Faltan referencias del VideoPlayer o ImagenFadeNegro.");
            return;
        }

        videoPlayer.isLooping = false;

        videoPlayer.prepareCompleted += AlPrepararVideo;
        videoPlayer.loopPointReached += AlTerminarVideo;

        videoPlayer.Prepare();
    }

    private void AlPrepararVideo(VideoPlayer reproductor)
    {
        reproductor.Play();

        // Apenas empieza el video, aclara desde negro (Fade In)
        corrutinaFadeActual = StartCoroutine(HacerFade(1f, 0f, duracionFadeEntrada));
    }

    private void Update()
    {
        if (videoPlayer == null || !videoPlayer.isPrepared)
            return;

        if (fadeIniciado || cambiandoEscena)
            return;

        if (videoPlayer.length > 0)
        {
            double tiempoRestante = videoPlayer.length - videoPlayer.time;

            if (videoPlayer.isPlaying && tiempoRestante <= duracionFade)
            {
                fadeIniciado = true;

                if (corrutinaFadeActual != null)
                    StopCoroutine(corrutinaFadeActual);

                // Al final del video, oscurece a negro (Fade Out)
                corrutinaFadeActual = StartCoroutine(HacerFade(imagenFadeNegro.color.a, 1f, duracionFade));
            }
        }
    }

    private IEnumerator HacerFade(float alphaInicial, float alphaFinal, float duracion)
    {
        float tiempo = 0f;
        Color color = imagenFadeNegro.color;

        while (tiempo < duracion)
        {
            tiempo += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float progreso = Mathf.Clamp01(tiempo / duracion);
            color.a = Mathf.SmoothStep(alphaInicial, alphaFinal, progreso);
            imagenFadeNegro.color = color;

            yield return null;
        }

        color.a = alphaFinal;
        imagenFadeNegro.color = color;
    }

    private void AlTerminarVideo(VideoPlayer reproductor)
    {
        if (cambiandoEscena) return;

        cambiandoEscena = true;
        SceneManager.LoadScene(escenaSiguiente);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= AlPrepararVideo;
            videoPlayer.loopPointReached -= AlTerminarVideo;
        }
    }
}
