using UnityEngine;

// Trigger en el tramo recto del pasillo. Está SIEMPRE activo: la condición (tener el fragmento)
// la comprueba PersecucionNiebla, así que pasar por acá antes de recogerlo no hace nada.
[RequireComponent(typeof(Collider))]
public class ZonaInicioPersecucion : MonoBehaviour
{
    [SerializeField] private PersecucionNiebla persecucion;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => Comprobar(other);

    // Stay cubre el caso de recoger el fragmento estando ya dentro de la zona
    private void OnTriggerStay(Collider other) => Comprobar(other);

    private void Comprobar(Collider other)
    {
        if (persecucion == null) return;
        if (other.GetComponentInParent<ControladorPersonaje>() == null) return;

        persecucion.IntentarIniciar();
    }
}