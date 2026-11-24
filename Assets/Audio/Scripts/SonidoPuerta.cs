using UnityEngine;

public class SonidoPuerta : MonoBehaviour
{
    [Header("Clips de audio")]
    public AudioClip sonidoAbrir;
    public AudioClip sonidoPortazo;

    [Header("Ajustes")]
    [Range(0f, 1f)]
    public float volumenAbrir = 0.85f;

    [Range(0f, 1f)]
    public float volumenPortazo = 1f;

    [Range(0f, 0.1f)]
    public float variacionPitch = 0.04f;

    [Tooltip("0 = 2D (se escucha siempre igual). 1 = 3D (sale desde la puerta).")]
    [Range(0f, 1f)]
    public float espacial3D = 0f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = espacial3D;
        _audioSource.minDistance = 2.5f;
        _audioSource.maxDistance = 20f;
    }

    public void ReproducirAbrir()
    {
        Reproducir(sonidoAbrir, volumenAbrir);
    }

    public void ReproducirPortazo()
    {
        Reproducir(sonidoPortazo, volumenPortazo);
    }

    private void Reproducir(AudioClip clip, float volumen)
    {
        if (clip == null) return;

        _audioSource.spatialBlend = espacial3D;
        _audioSource.pitch = Random.Range(1f - variacionPitch, 1f + variacionPitch);
        _audioSource.PlayOneShot(clip, volumen);
    }
}