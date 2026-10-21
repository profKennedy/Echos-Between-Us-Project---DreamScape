using System;
using System.Collections.Generic;
using UnityEngine;

public class MochilaDani : MonoBehaviour
{
    [SerializeField] private int capacidad = 10;
    [Tooltip("Opcional: dónde cae un item al descartarlo. Si está vacío, cae delante de Dani.")]
    [SerializeField] private Transform puntoDescarte;

    private readonly List<DatosItem> _items = new List<DatosItem>();

    // Objetos únicos que viven guardados (desactivados) en el mundo, como la linterna.
    // Como cada item es único, alcanza con indexarlos por su DatosItem.
    private readonly Dictionary<DatosItem, IObjetoPersistente> _persistentes = new Dictionary<DatosItem, IObjetoPersistente>();

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
        Instantiate(item.prefabMundo, posicion, Quaternion.identity);
        return true;
    }

    // Único lugar que saca items: mantiene lista y diccionario sincronizados
    private void QuitarEn(int indice)
    {
        DatosItem item = _items[indice];
        _items.RemoveAt(indice);
        _persistentes.Remove(item);
        OnMochilaCambiada?.Invoke();
    }

    private Vector3 PosicionDescarte()
    {
        if (puntoDescarte != null) return puntoDescarte.position;
        return transform.position + transform.forward * 1f + Vector3.up * 0.5f;
    }
}