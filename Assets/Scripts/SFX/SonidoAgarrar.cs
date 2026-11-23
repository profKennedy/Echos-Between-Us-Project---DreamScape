using UnityEngine;

public class SonidoAgarrar : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoClickUniversal;

    [Range(0f, 1f)]
    public float volumen = 0.8f;

    [Range(0f, 0.15f)]
    public float variacionPitch = 0.05f;

    // Se llama directo desde un Animation Event en el clip de recoger
    public void ReproducirClick()
    {
        if (sonidoClickUniversal == null || audioSource == null) return;

        audioSource.pitch = Random.Range(1f - variacionPitch, 1f + variacionPitch);
        audioSource.PlayOneShot(sonidoClickUniversal, volumen);
    }
}
