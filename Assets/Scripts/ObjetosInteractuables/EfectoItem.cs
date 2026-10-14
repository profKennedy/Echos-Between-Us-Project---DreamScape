using UnityEngine;

// Clase base de los efectos que un ítem puede tener al usarse desde la mochila (botón "Usar").
// Cada efecto nuevo (curar, abrir algo, etc.) es una clase hija + un asset, sin tocar la mochila.
public abstract class EfectoItem : ScriptableObject
{
    // Devuelve true si el efecto se aplicó de verdad.
    // Solo en ese caso la mochila consume el ítem (si es consumible).
    // 'mensaje' es el aviso para mostrar en la interfaz.
    public abstract bool Aplicar(ControladorPersonaje jugador, out string mensaje);
}