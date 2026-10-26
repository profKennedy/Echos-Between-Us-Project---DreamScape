using System;
using System.Collections.Generic;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instancia { get; private set; }

    private readonly HashSet<string> puzzlesCompletados =
        new HashSet<string>();

    public event Action<string> AlCompletarPuzzle;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
    }

    public void RegistrarPuzzleCompletado(string idPuzzle)
    {
        if (string.IsNullOrWhiteSpace(idPuzzle))
        {
            Debug.LogWarning("Se intento registrar un puzzle sin ID.");
            return;
        }

        if (!puzzlesCompletados.Add(idPuzzle))
        {
            Debug.Log("El puzzle ya estaba completado: " + idPuzzle);
            return;
        }

        Debug.Log("PROGRESO: Puzzle completado -> " + idPuzzle);

        AlCompletarPuzzle?.Invoke(idPuzzle);
    }

    public bool EstaCompletado(string idPuzzle)
    {
        return puzzlesCompletados.Contains(idPuzzle);
    }
}