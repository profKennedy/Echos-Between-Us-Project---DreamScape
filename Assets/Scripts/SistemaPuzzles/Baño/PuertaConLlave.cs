using UnityEngine;
using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;

public class PuertaConLlave : MonoBehaviour, IInteractable
{
    [Header("Configuracion")]
    [SerializeField] private DatosItem llaveNecesaria;

    [Header("Referencias")]
    [SerializeField] private GameObject puerta;
    [SerializeField] private GameObject cartelInteraccion;

    [Header("Opciones")]
    [SerializeField] private bool consumirLlave = true;

    private bool abierta = false;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (abierta || jugador == null || jugador.mochila == null)
            return;

        if (!jugador.mochila.Contiene(llaveNecesaria))
        {
            Debug.Log("La puerta esta cerrada. Necesitas una llave.");
            return;
        }

        if (consumirLlave)
        {
            if (!jugador.mochila.TryQuitar(llaveNecesaria))
                return;
        }

        AbrirPuerta();
    }

    private void AbrirPuerta()
    {
        abierta = true;

        MostrarPista(false);

        if (puerta != null)
            puerta.SetActive(false);

        Debug.Log("Puerta abierta.");
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelInteraccion != null)
            cartelInteraccion.SetActive(mostrar && !abierta);
    }
}