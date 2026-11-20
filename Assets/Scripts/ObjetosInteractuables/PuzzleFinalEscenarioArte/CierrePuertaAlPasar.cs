using UnityEngine;

// Trigger ubicado unos metros DESPUÉS de la puerta (del lado de afuera). Cuando Dani lo cruza, la puerta se
// cierra de golpe. Como el trigger está lejos del marco, Dani ya pasó y no queda en el camino de la hoja.
// Va en un objeto con un Collider (se pone solo en modo Trigger).
[RequireComponent(typeof(Collider))]
public class CierrePuertaAlPasar : MonoBehaviour
{
    [SerializeField] private Puerta puerta;

    [Tooltip("Opcional: si se asigna, la puerta solo se cierra cuando la persecución ya está completada (Dani escapó).")]
    [SerializeField] private PersecucionNiebla persecucion;

    private bool _disparado;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => Comprobar(other);

    // Stay cubre el caso de que la condición (persecución completada) se cumpla estando ya dentro de la zona
    private void OnTriggerStay(Collider other) => Comprobar(other);

    private void Comprobar(Collider other)
    {
        if (_disparado || puerta == null) return;
        if (other.GetComponentInParent<ControladorPersonaje>() == null) return;
        if (persecucion != null && persecucion.Estado != EstadoPersecucion.Completada) return;

        _disparado = true;
        puerta.CerrarDeGolpe();
    }
}