
using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    [Header("Identificacion")]
    [SerializeField] private string idPuzzle = "GIMNASIO_01";

    [Header("Estado")]
    [SerializeField] private bool completado = false;

    [Header("Eventos")]
    public UnityEvent alCompletarPuzzle;

    public bool EstaCompletado => completado;
    public string IdPuzzle => idPuzzle;

    public void CompletarPuzzle()
    {
        // Evita ejecutar varias veces el mismo puzzle
        if (completado)
            return;

        completado = true;

        Debug.Log("Puzzle completado: " + idPuzzle);

        // Notifica a puertas, luces, sonidos, blabla
        alCompletarPuzzle?.Invoke();
    }
}
