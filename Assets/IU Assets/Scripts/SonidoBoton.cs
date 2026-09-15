using UnityEngine;

public class SonidoBoton : MonoBehaviour
{
    public AudioSource fuenteDeAudio;
    public AudioClip sonidoClick;

    public void ReproducirClick()
    {
        fuenteDeAudio.PlayOneShot(sonidoClick);
    }
}