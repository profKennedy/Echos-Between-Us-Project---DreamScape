using System;
using System.Collections;
using UnityEngine;

// Un reflector del techo. YA NO es interactuable: no sabe quién lo enciende.
// Lo enciende un InterruptorReflector (o cualquier otro script) llamando a Encender().
// Se prende con un parpadeo corto y recién cuando termina avisa por OnEncendido (lo escucha PuzzleReflectores).
public class ReflectorEscenario : MonoBehaviour
{
    [Tooltip("Todo lo que se prende con el reflector: la Light, el cono de luz, un material emisivo... Arrancan apagados. NO pongas acá el objeto que tiene este script.")]
    [SerializeField] private GameObject[] objetosAlEncender;

    [Header("Parpadeo al encender")]
    [SerializeField] private float segundosParpadeo = 1.5f;
    [SerializeField] private float intervaloParpadeo = 0.12f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxEncender;

    private bool _encendiendo;

    // True recién cuando terminó el parpadeo y quedó prendido fijo
    public bool EstaEncendido { get; private set; }

    public event Action<ReflectorEscenario> OnEncendido;

    private void Awake()
    {
        AlternarObjetos(false);
    }

    public void Encender()
    {
        if (EstaEncendido || _encendiendo) return;

        _encendiendo = true;
        if (audioSource != null && sfxEncender != null) audioSource.PlayOneShot(sfxEncender);

        StartCoroutine(RutinaEncendido());
    }

    private IEnumerator RutinaEncendido()
    {
        float intervalo = Mathf.Max(0.02f, intervaloParpadeo);
        float total = Mathf.Max(intervalo, segundosParpadeo);
        float transcurrido = 0f;

        // Arranca apagado, con destellos cortos, y la luz "agarra" cada vez más seguido hasta quedar fija
        AlternarObjetos(false);

        while (transcurrido < total)
        {
            float progreso = transcurrido / total;
            float probabilidadEncendido = Mathf.Lerp(0.2f, 0.85f, progreso);
            AlternarObjetos(UnityEngine.Random.value < probabilidadEncendido);

            float espera = Mathf.Min(UnityEngine.Random.Range(intervalo * 0.5f, intervalo * 1.5f), total - transcurrido);
            espera = Mathf.Max(0.01f, espera);
            yield return new WaitForSeconds(espera);
            transcurrido += espera;
        }

        AlternarObjetos(true);
        EstaEncendido = true;
        _encendiendo = false;

        OnEncendido?.Invoke(this);
    }

    private void AlternarObjetos(bool encendido)
    {
        foreach (GameObject objeto in objetosAlEncender)
        {
            if (objeto != null && objeto != gameObject) objeto.SetActive(encendido);
        }
    }
}