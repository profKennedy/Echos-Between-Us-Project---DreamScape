using UnityEngine;

// Trigger justo DESPUÉS de la puerta (del lado de afuera). Cuando Dani lo cruza, escapó.
[RequireComponent(typeof(Collider))]
public class ZonaSalidaPersecucion : MonoBehaviour
{
    [SerializeField] private PersecucionNiebla persecucion;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (persecucion == null) return;
        if (other.GetComponentInParent<ControladorPersonaje>() == null) return;

        persecucion.Terminar();
    }
}