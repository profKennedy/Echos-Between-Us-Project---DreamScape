using UnityEngine;
using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;

public class ObjetoProgreso : MonoBehaviour, IInteractable
{
    [Header("Progreso")]
    [SerializeField] private string idProgreso;

    [Header("Interfaz")]
    [SerializeField] private GameObject cartelInteraccion;

    [Header("Configuracion")]
    [SerializeField] private bool desaparecerAlInteractuar = true;

    private bool utilizado = false;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (utilizado)
            return;

        if (GameProgressManager.Instancia == null)
        {
            Debug.LogError("No se encontro GameProgressManager.");
            return;
        }

        utilizado = true;

        // Registrar el progreso.
        GameProgressManager.Instancia
            .RegistrarPuzzleCompletado(idProgreso);

        Debug.Log("Objeto de progreso activado: " + idProgreso);

        // Ocultar cartel.
        MostrarPista(false);

        // Desaparecer el objeto.
        if (desaparecerAlInteractuar)
        {
            gameObject.SetActive(false);
        }
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelInteraccion != null)
        {
            cartelInteraccion.SetActive(mostrar && !utilizado);
        }
    }
}