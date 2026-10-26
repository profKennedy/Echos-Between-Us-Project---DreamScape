using UnityEngine;

public class DesbloqueablePorPuzzle : MonoBehaviour
{
    [Header("Puzzle necesario")]
    [SerializeField] private string idPuzzleRequerido;

    [Header("Objeto que bloquea")]
    [SerializeField] private GameObject objetoBloqueador;

    private GameProgressManager progreso;
    private bool desbloqueado;

    private void Start()
    {
        progreso = GameProgressManager.Instancia;

        if (progreso == null)
        {
            Debug.LogError(
                "No se encontro GameProgressManager.",
                this
            );
            return;
        }

        // Escuchamos cuando se completa un puzzle.
        progreso.AlCompletarPuzzle += VerificarProgreso;

        // Comprobamos si ya estaba completado.
        if (progreso.EstaCompletado(idPuzzleRequerido))
        {
            Desbloquear();
        }
    }

    private void OnDestroy()
    {
        if (progreso != null)
        {
            progreso.AlCompletarPuzzle -= VerificarProgreso;
        }
    }

    private void VerificarProgreso(string idPuzzle)
    {
        if (desbloqueado)
            return;

        if (idPuzzle == idPuzzleRequerido)
        {
            Desbloquear();
        }
    }

    private void Desbloquear()
    {
        if (desbloqueado)
            return;

        desbloqueado = true;

        if (objetoBloqueador != null)
        {
            objetoBloqueador.SetActive(false);
        }

        Debug.Log(
            "Objeto desbloqueado por: " + idPuzzleRequerido
        );
    }
}