using System.Collections;
using UnityEngine;

public class SonidoAgarrar : MonoBehaviour
{
    [Header("Solo arrastrá tu audio acá")]
    public AudioClip sonidoClick;

    [Header("Ajustes (opcional)")]
    [Range(0f, 1f)]
    public float volumen = 0.8f;

    [Range(0f, 0.15f)]
    public float variacionPitch = 0.05f;

    [Tooltip("Subilo de a poco si querés que suene justo cuando la mano toca el objeto")]
    [Range(0f, 1.5f)]
    public float retrasoSegundos = 0f;

    private InteraccionPersonaje _interaccion;
    private AudioSource _audioSource;

    private void Awake()
    {
        // Busca el script de tu compañero en este objeto o en el padre
        _interaccion = GetComponent<InteraccionPersonaje>();
        if (_interaccion == null)
            _interaccion = GetComponentInParent<InteraccionPersonaje>();

        // Crea su propio AudioSource en 2D para no pelear con otros audios
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        if (_interaccion != null)
            _interaccion.OnLevantarObjeto += AlLevantar;
        else
            Debug.LogWarning("[SonidoAgarrar] No encontré InteraccionPersonaje en este objeto.");
    }

    private void OnDisable()
    {
        if (_interaccion != null)
            _interaccion.OnLevantarObjeto -= AlLevantar;
    }

    private void AlLevantar()
    {
        StartCoroutine(Reproducir());
    }

    private IEnumerator Reproducir()
    {
        if (retrasoSegundos > 0f)
            yield return new WaitForSeconds(retrasoSegundos);

        if (sonidoClick == null)
        {
            Debug.LogWarning("[SonidoAgarrar] Falta arrastrar el archivo de audio en Sonido Click.");
            yield break;
        }

        _audioSource.pitch = Random.Range(1f - variacionPitch, 1f + variacionPitch);
        _audioSource.PlayOneShot(sonidoClick, volumen);
        Debug.Log("[SonidoAgarrar] Click reproducido.");
    }
}