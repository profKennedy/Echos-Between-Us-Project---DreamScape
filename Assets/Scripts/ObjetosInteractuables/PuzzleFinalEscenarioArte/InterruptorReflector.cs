using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System.Collections;
using UnityEngine;

// Interruptor de pared de UN reflector. Dani lo activa con E: la palanca gira y el reflector se enciende.
// Es de un solo uso: al activarse apaga sus propios colliders, así InteraccionPersonaje ya no lo detecta.
// Va en el MISMO objeto que tiene el Collider (en la capa Interactuable).
public class InterruptorReflector : MonoBehaviour, IInteractable
{
    [Header("Qué enciende")]
    [SerializeField] private ReflectorEscenario reflector;

    [Header("Palanca (opcional)")]
    [Tooltip("El objeto que gira. Su pivote tiene que estar en el punto donde gira la palanca.")]
    [SerializeField] private Transform palanca;
    [Tooltip("Cuánto gira (en grados, relativo a su rotación inicial) cuando se activa.")]
    [SerializeField] private Vector3 rotacionActivada = new Vector3(-50f, 0f, 0f);
    [SerializeField] private float segundosGiro = 0.2f;

    [Header("UI")]
    [SerializeField] private GameObject cartelUI;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxClick;

    private bool _usado;
    private Quaternion _rotacionInicial;

    private void Awake()
    {
        if (palanca != null) _rotacionInicial = palanca.localRotation;

        if (reflector == null)
            Debug.LogError("InterruptorReflector: falta asignar el reflector que enciende.", this);
    }

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (_usado || reflector == null) return;

        _usado = true;
        MostrarPista(false);

        // Sin colliders ya no es detectable como objeto cercano: no vuelve a ofrecer "E"
        foreach (Collider c in GetComponents<Collider>()) c.enabled = false;

        StartCoroutine(RutinaActivar());
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelUI != null) cartelUI.SetActive(mostrar && !_usado);
    }

    private IEnumerator RutinaActivar()
    {
        if (audioSource != null && sfxClick != null) audioSource.PlayOneShot(sfxClick);

        if (palanca != null)
        {
            Quaternion hasta = _rotacionInicial * Quaternion.Euler(rotacionActivada);
            float duracion = Mathf.Max(0.01f, segundosGiro);
            float t = 0f;

            while (t < duracion)
            {
                t += Time.deltaTime;
                palanca.localRotation = Quaternion.Slerp(_rotacionInicial, hasta, t / duracion);
                yield return null;
            }

            palanca.localRotation = hasta;
        }

        // Recién con la palanca abajo se prende la luz
        reflector.Encender();
    }

    // En la Scene view: línea amarilla del interruptor a su reflector, para no cruzar los 4 pares
    private void OnDrawGizmosSelected()
    {
        if (reflector == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, reflector.transform.position);
    }
}