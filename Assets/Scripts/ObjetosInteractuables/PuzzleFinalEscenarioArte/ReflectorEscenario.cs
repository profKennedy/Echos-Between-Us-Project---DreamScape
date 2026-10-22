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
    [SerializeField] private float segundosParpadeo = 0.6f;
    [SerializeField] private float intervaloParpadeo = 0.08f;

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
        float restante = segundosParpadeo;

        while (restante > 0f)
        {
            AlternarObjetos(UnityEngine.Random.value > 0.5f);

            float espera = Mathf.Min(intervalo, restante);
            yield return new WaitForSeconds(espera);
            restante -= espera;
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