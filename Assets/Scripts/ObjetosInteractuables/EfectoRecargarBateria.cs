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

        // Por ahora la linterna tiene que estar en la mano.
        // Cuando la linterna pueda guardarse en la mochila, acá se acepta también "en la mochila".
        if (gestion.LinternaDisponibleEnSuelo())
        {
            mensaje = "Necesitás tener la linterna en la mano.";
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