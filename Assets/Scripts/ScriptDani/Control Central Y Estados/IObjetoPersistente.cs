using UnityEngine;

// Objeto ÚNICO cuyo GameObject sobrevive mientras está en la mochila (se desactiva, no se destruye)
// y conserva su estado (ej: la batería de la linterna).
// Al descartarlo, la mochila NO crea una copia desde un prefab: le pide que vuelva al mundo.
public interface IObjetoPersistente
{
    void AlDescartarDeMochila(Vector3 posicion);
}