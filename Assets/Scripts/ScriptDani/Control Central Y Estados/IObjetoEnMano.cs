// Algo que Dani puede tener en la mano. De a UNO por vez: lo controla ManoDani.
public interface IObjetoEnMano
{
    DatosItem Datos { get; }

    // Lo llama el botón "Guardar" de la mochila. Devuelve true si lo guardó.
    bool TryGuardar(MochilaDani mochila, out string mensaje);
}