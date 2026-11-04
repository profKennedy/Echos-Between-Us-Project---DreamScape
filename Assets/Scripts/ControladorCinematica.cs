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
    [SerializeField] private float duracionFade = 2f;

    [Header("Escena siguiente")]
    [SerializeField] private string escenaSiguiente = "EscenaVerticalSlice";

    private bool cambiandoEscena = false;
    private bool fadeIniciado = false;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        if (videoPlayer == null || imagenFadeNegro == null)
        {
            Debug.LogError("Faltan referencias del VideoPlayer o ImagenFadeNegro.");
            return;
        }

        // Empezar con la imagen transparente.
        Color color = imagenFadeNegro.color;
        color.a = 0f;
        imagenFadeNegro.color = color;

        videoPlayer.isLooping = false;

        videoPlayer.prepareCompleted += AlPrepararVideo;
        videoPlayer.loopPointReached += AlTerminarVideo;

        videoPlayer.Prepare();
    }

    private void AlPrepararVideo(VideoPlayer reproductor)
    {
        reproductor.Play();
    }

    private void Update()
    {
        if (videoPlayer == null || !videoPlayer.isPrepared)
            return;

        if (fadeIniciado || cambiandoEscena)
            return;

        // Comprobar cuánto falta para terminar.
        if (videoPlayer.length > 0)
        {
            double tiempoRestante = videoPlayer.length - videoPlayer.time;

            if (videoPlayer.isPlaying && tiempoRestante <= duracionFade)
            {
                fadeIniciado = true;
                StartCoroutine(FadeANegro());
            }
        }
    }

    private IEnumerator FadeANegro()
    {
        float tiempo = 0f;

        Color color = imagenFadeNegro.color;

        while (tiempo < duracionFade)
        {
            tiempo += Time.unscaledDeltaTime;

            color.a = Mathf.Clamp01(tiempo / duracionFade);
            imagenFadeNegro.color = color;

            yield return null;
        }

        color.a = 1f;
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