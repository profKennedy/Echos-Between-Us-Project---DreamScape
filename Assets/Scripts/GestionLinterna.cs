using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using UnityEngine;

public enum UbicacionLinterna
{
    EnSuelo,
    EnMano,
    EnMochila
}

public class GestionLinterna : MonoBehaviour, IObjetoLevantable, IObjetoEnMano, IObjetoPersistente
{
    public ControladorLinterna linterna;
    public GameObject cartelUI;     // cartel "E - Recoger"
    public Transform transformMano;

    [Header("Mochila")]
    [Tooltip("Asset DatosItem de la linterna: es lo que aparece en la mochila.")]
    [SerializeField] private DatosItem datosItem;

    private UbicacionLinterna _ubicacion = UbicacionLinterna.EnSuelo; // Nivel 1: arranca en el suelo
    private Transform _padreOriginal; // para restaurar al soltarla o guardarla
    private ManoDani _mano;
    private Rigidbody _rb;
    private Collider[] _colliders;

    public DatosItem Datos => datosItem;
    public bool EstaEnMano => _ubicacion == UbicacionLinterna.EnMano;
    public bool EstaEnMochila => _ubicacion == UbicacionLinterna.EnMochila;
    public bool EstaPoseida => _ubicacion != UbicacionLinterna.EnSuelo;

    private void Awake()
    {
        linterna = GetComponent<ControladorLinterna>();
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>(true);
    }

    // Compatibilidad con scripts que ya la consultaban
    public bool LinternaDisponibleEnSuelo() => _ubicacion == UbicacionLinterna.EnSuelo;

    // ---------------------------------------------------------------
    // Recoger del suelo (llega acá después de la animación de levantar)
    // ---------------------------------------------------------------
    public void Interactuar(ControladorPersonaje jugador)
    {
        if (_ubicacion != UbicacionLinterna.EnSuelo) return;

        if (!Equipar(jugador, out string mensaje))
            Debug.Log(mensaje);
    }

    public void MostrarPista(bool mostrar)
    {
        // Solo se ofrece "recoger" si está en el suelo
        if (cartelUI != null) cartelUI.SetActive(mostrar && _ubicacion == UbicacionLinterna.EnSuelo);
    }

    // ---------------------------------------------------------------
    // Poner la linterna en la mano (desde el suelo o desde la mochila)
    // ---------------------------------------------------------------
    public bool Equipar(ControladorPersonaje jugador, out string mensaje)
    {
        mensaje = "";
        if (_ubicacion == UbicacionLinterna.EnMano) return false;

        _mano = jugador.mano;
        transformMano = jugador.transformMano;

        if (_mano == null)
        {
            Debug.LogWarning("ControladorPersonaje no tiene 'mano' asignada: no se controla que haya un solo objeto en la mano.", this);
        }
        else if (!_mano.TryPoner(this))
        {
            mensaje = "Ya tenés algo en la mano.";
            return false;
        }

        // El padre original se guarda una sola vez: cuando la agarramos del suelo
        if (_ubicacion == UbicacionLinterna.EnSuelo) _padreOriginal = linterna.transform.parent;

        linterna.gameObject.SetActive(true);
        linterna.transform.SetParent(transformMano);
        linterna.transform.localPosition = Vector3.zero;
        linterna.transform.localRotation = Quaternion.identity;

        // En la mano NO debe ser detectable como "objeto cercano" ni tener física
        ActivarFisica(false);
        MostrarPista(false);

        _ubicacion = UbicacionLinterna.EnMano;
        Debug.Log("Linterna equipada.");
        return true;
    }

    // ---------------------------------------------------------------
    // Mano -> mochila (botón "Guardar")
    // ---------------------------------------------------------------
    public bool TryGuardar(MochilaDani mochila, out string mensaje)
    {
        mensaje = "";
        if (_ubicacion != UbicacionLinterna.EnMano) return false;

        if (datosItem == null)
        {
            Debug.LogError("GestionLinterna: falta asignar 'Datos Item' (el asset de la linterna).", this);
            mensaje = "La linterna no tiene datos asignados.";
            return false;
        }

        // Es lo único que puede fallar (mochila llena), por eso va primero
        if (!mochila.TryAgregar(datosItem, this))
        {
            mensaje = "La mochila está llena.";
            return false;
        }

        if (linterna.EstaEncendida()) linterna.Apagar();

        // No se destruye: se desactiva y conserva su batería
        linterna.transform.SetParent(_padreOriginal);
        linterna.gameObject.SetActive(false);
        _ubicacion = UbicacionLinterna.EnMochila;

        if (_mano != null) _mano.Liberar(this);

        mensaje = "Linterna guardada.";
        return true;
    }

    // ---------------------------------------------------------------
    // Mochila -> mundo (botón "Descartar" sobre la linterna)
    // ---------------------------------------------------------------
    public void AlDescartarDeMochila(Vector3 posicion)
    {
        if (_ubicacion != UbicacionLinterna.EnMochila) return;

        linterna.transform.SetParent(_padreOriginal);
        linterna.transform.position = posicion;
        linterna.transform.rotation = Quaternion.identity;
        linterna.gameObject.SetActive(true);

        ActivarFisica(true);
        _ubicacion = UbicacionLinterna.EnSuelo;
    }

    // ---------------------------------------------------------------
    // Soltarla (susto)
    // ---------------------------------------------------------------
    public void SoltarLinterna()
    {
        // Si no está en la mano (suelo o mochila), no hay nada que soltar
        if (_ubicacion != UbicacionLinterna.EnMano) return;

        linterna.transform.SetParent(_padreOriginal);
        ActivarFisica(true);

        if (linterna.EstaEncendida()) linterna.Apagar();

        _ubicacion = UbicacionLinterna.EnSuelo;
        if (_mano != null) _mano.Liberar(this);

        Debug.Log("Linterna soltada.");
    }

    public void SoltarLinternaPorSusto()
    {
        // En la mochila o en el suelo, el susto no la afecta
        if (_ubicacion != UbicacionLinterna.EnMano) return;

        SoltarLinterna();

        // Impulso aleatorio al soltar por susto
        if (_rb != null)
        {
            Vector3 impulso = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(-1f, 1f)
            ).normalized * 3f;
            _rb.AddForce(impulso, ForceMode.Impulse);
        }

        Debug.Log("¡Dani soltó la linterna por el susto!");
    }

    public void IntentarAlternarLuz()
    {
        if (_ubicacion != UbicacionLinterna.EnMano) return; // solo se enciende si está en la mano
        linterna.AlternarLuz();
    }

    // Física + colliders: apagados en la mano (si no, sigue siendo detectable como objeto cercano)
    private void ActivarFisica(bool activa)
    {
        if (_rb != null)
        {
            _rb.isKinematic = !activa;
            _rb.detectCollisions = activa;
            if (activa) _rb.velocity = Vector3.zero;
        }

        foreach (Collider c in _colliders)
            c.enabled = activa;
    }
}