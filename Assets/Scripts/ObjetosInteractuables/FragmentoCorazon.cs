using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System;
using UnityEngine;

// El fragmento de corazón: objeto ÚNICO que vive en el mundo, igual que la linterna.
// Al recogerlo se guarda en la mochila (no se consume). Si Dani falla la persecución,
// PersecucionNiebla lo saca de la mochila y lo manda de vuelta a su lugar original.
public class FragmentoCorazon : MonoBehaviour, IObjetoLevantable, IObjetoPersistente
{
    [Tooltip("Asset DatosItem del fragmento: es lo que aparece en la mochila y lo que pide ZonaFinal.")]
    [SerializeField] private DatosItem datosItem;

    // Mismo nombre que antes: la referencia del Inspector se conserva
    [SerializeField] private GameObject cartelUI;

    private Vector3 _posicionOrigen;
    private Quaternion _rotacionOrigen;

    public DatosItem Datos => datosItem;

    // Para efectos de sonido, música, etc. (PersecucionNiebla NO lo necesita: la zona de inicio mira la mochila)
    public event Action OnRecogido;

    private void Awake()
    {
        _posicionOrigen = transform.position;
        _rotacionOrigen = transform.rotation;
    }

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (datosItem == null)
        {
            Debug.LogError("FragmentoCorazon: falta asignar 'Datos Item'.", this);
            return;
        }

        if (jugador.mochila == null)
        {
            Debug.LogError("ControladorPersonaje no tiene la mochila asignada.", jugador);
            return;
        }

        // Se pasa a sí mismo: si Dani lo descarta, vuelve ESTE objeto, no una copia
        if (!jugador.mochila.TryAgregar(datosItem, this))
        {
            Debug.Log("Mochila llena: no se puede recoger el fragmento.");
            return;
        }

        MostrarPista(false);
        gameObject.SetActive(false);
        OnRecogido?.Invoke();
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelUI != null) cartelUI.SetActive(mostrar);
    }

    // Mochila -> mundo (botón "Descartar")
    public void AlDescartarDeMochila(Vector3 posicion)
    {
        transform.position = posicion;
        gameObject.SetActive(true);
    }

    // Reinicio de la persecución: sale de la mochila (si estaba) y vuelve a su lugar
    public void VolverAlOrigen(MochilaDani mochila)
    {
        if (mochila != null) mochila.TryQuitar(datosItem);

        transform.SetPositionAndRotation(_posicionOrigen, _rotacionOrigen);
        gameObject.SetActive(true);
    }
}