
using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using UnityEngine;

public class ObjetoRecogible : MonoBehaviour, IObjetoLevantable
{
    [Header("Inventario")]
    [SerializeField] private DatosItem datos;

    [Header("Interfaz")]
    [SerializeField] private GameObject cartelUI;

    [Header("Progreso (opcional)")]
    [SerializeField] private bool registrarProgreso = false;
    [SerializeField] private string idProgreso;

    private bool recogido = false;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (recogido)
            return;

        if (datos == null)
        {
            Debug.LogError(
                "ObjetoRecogible sin DatosItem asignado.", this);
            return;
        }

        if (jugador == null || jugador.mochila == null)
        {
            Debug.LogError(
                "El jugador no tiene mochila asignada.", this);
            return;
        }

        // Comprobar que el progreso pueda registrarse.
        if (registrarProgreso)
        {
            if (GameProgressManager.Instancia == null)
            {
                Debug.LogError(
                    "No se encontro GameProgressManager.", this);
                return;
            }

            if (string.IsNullOrWhiteSpace(idProgreso))
            {
                Debug.LogError(
                    "No se asigno un ID de progreso.", this);
                return;
            }
        }

        // Primero guardamos el objeto en la mochila.
        if (!jugador.mochila.TryAgregar(datos))
        {
            Debug.Log(
                $"Mochila llena: no se puede recoger '{datos.nombre}'.");
            return;
        }

        recogido = true;

        // Si se guardó correctamente, registramos el progreso.
        if (registrarProgreso)
        {
            GameProgressManager.Instancia
                .RegistrarPuzzleCompletado(idProgreso);

            Debug.Log(
                "Progreso registrado al recoger: " + idProgreso);
        }

        MostrarPista(false);

        Destroy(gameObject);
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelUI != null)
            cartelUI.SetActive(mostrar && !recogido);
    }
}
