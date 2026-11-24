using System;
using System.Collections.Generic;
using UnityEngine;

public class MochilaDani : MonoBehaviour
{
    [SerializeField] private int capacidad = 10;
    [Tooltip("Opcional: dónde cae un item al descartarlo. Si está vacío, cae delante de Dani.")]
    [SerializeField] private Transform puntoDescarte;

    [Header("Descarte")]
    [Tooltip("A cuántos metros delante de Dani cae el objeto (se acorta si hay una pared).")]
    [SerializeField] private float distanciaDescarte = 1f;
    [Tooltip("Separación mínima entre el objeto y el piso al aparecer, para que no nazca incrustado.")]
    [SerializeField] private float alturaSobreElPiso = 0.15f;
    [Tooltip("Capas que cuentan como piso/pared al buscar dónde soltar el objeto.")]
    [SerializeField] private LayerMask capasPiso = ~0;
    [Tooltip("Si un objeto soltado cae más que esto por debajo de donde apareció (atravesó el piso), vuelve a su lugar.")]
    [SerializeField] private float caidaMaximaRecuperable = 5f;

    private readonly List<DatosItem> _items = new List<DatosItem>();

    // Objetos únicos que viven guardados (desactivados) en el mundo, como la linterna.
    // Como cada item es único, alcanza con indexarlos por su DatosItem.
    private readonly Dictionary<DatosItem, IObjetoPersistente> _persistentes = new Dictionary<DatosItem, IObjetoPersistente>();

    // Tamaño EN EL MUNDO que tenía cada objeto cuando Dani lo recogió.
    // Los objetos de la escuela viven dentro de padres escalados (Escuela x0.5, Gimnasio x5, etc.),
    // pero el prefab se guarda con la escala LOCAL: si no se recuerda, al soltarlo sale de otro tamaño.
    private readonly Dictionary<DatosItem, Vector3> _escalasMundo = new Dictionary<DatosItem, Vector3>();

    public event Action OnMochilaCambiada;

    public int Capacidad => capacidad;
    public IReadOnlyList<DatosItem> Items => _items;
    public bool EstaLlena => _items.Count >= capacidad;

    // 'persistente' solo se pasa para objetos únicos que conservan su estado (la linterna)
    public bool TryAgregar(DatosItem item, IObjetoPersistente persistente = null)
    {
        if (item == null || EstaLlena) return false;

        _items.Add(item);
        if (persistente != null) _persistentes[item] = persistente;

        OnMochilaCambiada?.Invoke();
        return true;
    }

    public bool Contiene(DatosItem item) => _items.Contains(item);

    // Lo llama ObjetoRecogible justo después de guardar el objeto
    public void RegistrarEscalaMundo(DatosItem item, Vector3 escala)
    {
        if (item == null) return;
        _escalasMundo[item] = escala;
    }

    // La interfaz lo usa para no ofrecer "Descartar" en objetos que no se pueden soltar
    public bool PuedeDescartar(int indice)
    {
        if (indice < 0 || indice >= _items.Count) return false;

        DatosItem item = _items[indice];
        return _persistentes.ContainsKey(item) || item.prefabMundo != null;
    }

    // Saca un item por referencia sin aplicar efectos (lo usa el reinicio de la persecución)
    public bool TryQuitar(DatosItem item)
    {
        int indice = _items.IndexOf(item);
        if (indice < 0) return false;

        QuitarEn(indice);
        return true;
    }

    // "Usar" un item buscándolo por referencia (lo usan las puertas con su llave).
    // Devuelve true si estaba en la mochila. Si es consumible, además se borra.
    public bool TryUsar(DatosItem item)
    {
        int indice = _items.IndexOf(item);
        if (indice < 0) return false;

        if (item.consumirAlUsar) QuitarEn(indice);
        return true;
    }

    // "Usar" un item desde el botón de la interfaz: aplica su efecto.
    // Solo se quita si el efecto se aplicó de verdad (ej: no se gasta una pila con la linterna llena).
    public bool UsarDesdeMochila(int indice, ControladorPersonaje jugador, out string mensaje)
    {
        mensaje = "";
        if (indice < 0 || indice >= _items.Count) return false;

        DatosItem item = _items[indice];

        if (item.efecto == null)
        {
            mensaje = "Este objeto no se usa desde la mochila.";
            return false;
        }

        if (!item.efecto.Aplicar(jugador, out mensaje)) return false;

        if (item.consumirAlUsar) QuitarEn(indice);
        return true;
    }

    public bool Descartar(int indice)
    {
        if (indice < 0 || indice >= _items.Count) return false;

        DatosItem item = _items[indice];
        Vector3 posicion = PosicionDescarte();

        // Objeto único (linterna): vuelve el MISMO objeto, con su batería, no una copia
        if (_persistentes.TryGetValue(item, out IObjetoPersistente persistente))
        {
            persistente.AlDescartarDeMochila(posicion);
            QuitarEn(indice);
            return true;
        }

        // Sin prefab, el item desaparecería para siempre (una llave descartada = juego imposible de terminar)
        if (item.prefabMundo == null)
        {
            Debug.LogWarning($"No se puede descartar '{item.nombre}': no tiene prefabMundo asignado.");
            return false;
        }

        QuitarEn(indice);

        GameObject objeto = Instantiate(item.prefabMundo, posicion, Quaternion.identity);

        // Mismo tamaño que tenía en el mundo cuando lo recogió
        if (_escalasMundo.TryGetValue(item, out Vector3 escala))
            objeto.transform.localScale = escala;

        PrepararFisica(objeto, posicion);
        return true;
    }

    // Los objetos son chiquitos y finos (una llave tiene ~3 cm de espesor): con la detección normal
    // pueden atravesar el piso al caer. Continuous lo evita, y ObjetoSoltado es la red de seguridad.
    private void PrepararFisica(GameObject objeto, Vector3 posicion)
    {
        if (!objeto.TryGetComponent(out Rigidbody rb)) return;

        if (!rb.isKinematic)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        ObjetoSoltado seguro = objeto.AddComponent<ObjetoSoltado>();
        seguro.Configurar(posicion, caidaMaximaRecuperable);
    }

    // Único lugar que saca items: mantiene lista y diccionario sincronizados
    private void QuitarEn(int indice)
    {
        DatosItem item = _items[indice];
        _items.RemoveAt(indice);
        _persistentes.Remove(item);
        OnMochilaCambiada?.Invoke();
    }

    // Antes: siempre 1 m adelante y 0,5 m arriba, sin mirar qué había ahí. Contra una pared el objeto
    // nacía adentro de la pared y se perdía. Ahora: se acorta si hay un obstáculo y se apoya sobre el piso.
    private Vector3 PosicionDescarte()
    {
        if (puntoDescarte != null) return puntoDescarte.position;

        Vector3 adelante = transform.forward;
        adelante.y = 0f;
        adelante = adelante.sqrMagnitude > 0.001f ? adelante.normalized : Vector3.forward;

        Vector3 origen = transform.position + Vector3.up * 1f;

        // 1. Hacia adelante, sin atravesar paredes
        float distancia = distanciaDescarte;
        if (RaycastIgnorandoADani(origen, adelante, distanciaDescarte + 0.3f, out RaycastHit pared))
            distancia = Mathf.Max(0f, pared.distance - 0.3f);

        Vector3 punto = origen + adelante * distancia;

        // 2. Hacia abajo hasta encontrar el piso
        if (RaycastIgnorandoADani(punto, Vector3.down, 3f, out RaycastHit piso))
            return piso.point + Vector3.up * alturaSobreElPiso;

        // 3. Sin piso a la vista: a los pies de Dani
        return transform.position + Vector3.up * 0.3f;
    }

    // El rayo sale desde adentro de Dani: se descartan sus propios colliders y los triggers
    private bool RaycastIgnorandoADani(Vector3 origen, Vector3 direccion, float distancia, out RaycastHit resultado)
    {
        RaycastHit[] hits = Physics.RaycastAll(origen, direccion, distancia, capasPiso, QueryTriggerInteraction.Ignore);

        resultado = default;
        float mejor = float.MaxValue;
        bool encontrado = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform.root)) continue;
            if (hit.distance >= mejor) continue;

            mejor = hit.distance;
            resultado = hit;
            encontrado = true;
        }

        return encontrado;
    }
}