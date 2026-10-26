using UnityEngine;

public class RegistrarProgresoPuzzle : MonoBehaviour
{
    [Header("Identificador unico")]
    [SerializeField] private string idPuzzle;

    public void RegistrarCompletado()
    {
        if (GameProgressManager.Instancia == null)
        {
            Debug.LogError(
                "No existe GameProgressManager en la escena.",
                this
            );
            return;
        }

        GameProgressManager.Instancia
            .RegistrarPuzzleCompletado(idPuzzle);
    }
}