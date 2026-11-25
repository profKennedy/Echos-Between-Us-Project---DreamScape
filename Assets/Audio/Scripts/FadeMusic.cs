using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeMusica : MonoBehaviour
{
    [Header("Referencias (dejar vacío para detectar todos automáticamente)")]
    public AudioSource[] fuentesAudio;

    [Header("Configuración de Fade")]
    public float duracionFade = 1.5f;
    public bool fadeInAlIniciar = true;

    private float[] volumenesBase;
    private float multiplicadorFade = 1f;
    private float multiplicadorVolumen = 1f;
    private Coroutine rutinaActual;

    private void Awake()
    {
        if (fuentesAudio == null || fuentesAudio.Length == 0)
            fuentesAudio = GetComponents<AudioSource>();

        volumenesBase = new float[fuentesAudio.Length];
        for (int i = 0; i < fuentesAudio.Length; i++)
        {
            if (fuentesAudio[i] != null)
                volumenesBase[i] = fuentesAudio[i].volume;
        }
    }

    private void Start()
    {
        if (fadeInAlIniciar && fuentesAudio.Length > 0)
        {
            multiplicadorFade = 0f;
            AplicarVolumenes();

            foreach (AudioSource fuente in fuentesAudio)
            {
                if (fuente != null && !fuente.isPlaying)
                    fuente.Play();
            }

            HacerFadeIn();
        }
    }

    // Llamar a esta línea desde cualquier script justo al empezar el fade a negro
    public static void ApagarMusicaActual()
    {
        FadeMusica[] fades = FindObjectsByType<FadeMusica>(FindObjectsSortMode.None);
        foreach (FadeMusica fade in fades)
        {
            fade.HacerFadeOut();
        }
    }

    public void HacerFadeIn()
    {
        IniciarFade(1f, false);
    }

    public void HacerFadeOut()
    {
        IniciarFade(0f, true);
    }

    // Para botones que cambian de escena directo desde On Click () sin otro script de fade
    public void FadeOutYCambiarEscena(string nombreEscena)
    {
        if (rutinaActual != null)
            StopCoroutine(rutinaActual);

        rutinaActual = StartCoroutine(RutinaSalirDeEscena(nombreEscena));
    }

    public void ActualizarVolumenObjetivo(float nuevoVolumen)
    {
        multiplicadorVolumen = Mathf.Clamp01(nuevoVolumen);
        AplicarVolumenes();
    }

    private void IniciarFade(float destino, bool pausarAlTerminar)
    {
        if (fuentesAudio == null || fuentesAudio.Length == 0) return;

        if (rutinaActual != null)
            StopCoroutine(rutinaActual);

        rutinaActual = StartCoroutine(RutinaFade(destino, pausarAlTerminar));
    }

    private IEnumerator RutinaSalirDeEscena(string nombreEscena)
    {
        yield return RutinaFade(0f, true);
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscena);
    }

    private IEnumerator RutinaFade(float destino, bool pausarAlTerminar)
    {
        float inicial = multiplicadorFade;
        float tiempo = 0f;

        if (destino > 0f)
        {
            foreach (AudioSource fuente in fuentesAudio)
            {
                if (fuente != null && !fuente.isPlaying)
                    fuente.Play();
            }
        }

        if (duracionFade > 0f)
        {
            while (tiempo < duracionFade)
            {
                tiempo += Time.unscaledDeltaTime;
                multiplicadorFade = Mathf.Lerp(inicial, destino, tiempo / duracionFade);
                AplicarVolumenes();
                yield return null;
            }
        }

        multiplicadorFade = destino;
        AplicarVolumenes();

        if (pausarAlTerminar && Mathf.Approximately(destino, 0f))
        {
            foreach (AudioSource fuente in fuentesAudio)
            {
                if (fuente != null)
                    fuente.Pause();
            }
        }

        rutinaActual = null;
    }

    private void AplicarVolumenes()
    {
        if (fuentesAudio == null || volumenesBase == null) return;

        for (int i = 0; i < fuentesAudio.Length; i++)
        {
            if (fuentesAudio[i] != null)
                fuentesAudio[i].volume = volumenesBase[i] * multiplicadorVolumen * multiplicadorFade;
        }
    }
}