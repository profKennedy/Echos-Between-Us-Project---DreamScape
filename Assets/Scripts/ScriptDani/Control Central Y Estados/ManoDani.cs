using System;
using UnityEngine;

// La mano de Dani: un solo objeto a la vez. Va en el mismo objeto que ControladorPersonaje.
// Hoy solo la usa la linterna; las pelotas y otros objetos levantables se enchufan acá después.
public class ManoDani : MonoBehaviour
{
    private IObjetoEnMano _actual;

    public event Action OnManoCambiada;

    public bool EstaVacia => _actual == null;
    public DatosItem ItemActual => _actual != null ? _actual.Datos : null;

    // false si ya hay algo en la mano
    public bool TryPoner(IObjetoEnMano objeto)
    {
        if (objeto == null || _actual != null) return false;

        _actual = objeto;
        OnManoCambiada?.Invoke();
        return true;
    }

    // Solo libera si el que pide es el que está en la mano
    public void Liberar(IObjetoEnMano objeto)
    {
        if (_actual == null || _actual != objeto) return;

        _actual = null;
        OnManoCambiada?.Invoke();
    }

    public bool TryGuardarEnMochila(MochilaDani mochila, out string mensaje)
    {
        mensaje = "No tenés nada en la mano.";
        if (_actual == null) return false;

        return _actual.TryGuardar(mochila, out mensaje);
    }
}