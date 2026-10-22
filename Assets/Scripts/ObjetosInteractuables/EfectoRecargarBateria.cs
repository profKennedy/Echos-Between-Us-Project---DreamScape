using UnityEngine;

[CreateAssetMenu(fileName = "EfectoRecargarBateria", menuName = "Echoes/Efectos/Recargar Bateria")]
public class EfectoRecargarBateria : EfectoItem
{
    [Tooltip("Cuánta batería suma. La linterna tiene 100 de máximo.")]
    [SerializeField] private float cantidad = 50f;

    public override bool Aplicar(ControladorPersonaje jugador, out string mensaje)
    {
        GestionLinterna gestion = jugador.gestionLinterna;

        if (gestion == null || gestion.linterna == null)
        {
            mensaje = "No tenés una linterna.";
            return false;
        }

        // Sirve con la linterna en la mano O guardada en la mochila. Si sigue en el suelo, no.
        if (!gestion.EstaPoseida)
        {
            mensaje = "Primero tenés que recoger la linterna.";
            return false;
        }

        ControladorLinterna linterna = gestion.linterna;

        if (linterna.bateriaActual >= linterna.bateriaMaxima)
        {
            mensaje = "La linterna ya está cargada.";
            return false;
        }

        linterna.RecargarBateria(cantidad);
        mensaje = $"Linterna recargada (+{cantidad:0}).";
        return true;
    }
}