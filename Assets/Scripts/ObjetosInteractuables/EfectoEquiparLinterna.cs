using UnityEngine;

// Efecto del ítem "Linterna" en la mochila: sacarla a la mano.
// Ese DatosItem lleva 'Consumir Al Usar' tildado, porque acá significa "sale de la mochila".
[CreateAssetMenu(fileName = "EfectoEquiparLinterna", menuName = "Echoes/Efectos/Equipar Linterna")]
public class EfectoEquiparLinterna : EfectoItem
{
    public override bool Aplicar(ControladorPersonaje jugador, out string mensaje)
    {
        GestionLinterna gestion = jugador.gestionLinterna;

        if (gestion == null)
        {
            mensaje = "No tenés una linterna.";
            return false;
        }

        if (!gestion.EstaEnMochila)
        {
            mensaje = "La linterna no está en la mochila.";
            return false;
        }

        if (!gestion.Equipar(jugador, out mensaje)) return false;

        mensaje = "Linterna en la mano.";
        return true;
    }
}