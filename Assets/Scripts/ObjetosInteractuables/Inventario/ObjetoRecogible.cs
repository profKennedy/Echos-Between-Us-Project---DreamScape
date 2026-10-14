using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using UnityEngine;

public class ObjetoRecogible : MonoBehaviour, IObjetoLevantable
{
    [SerializeField] private DatosItem datos;
    [SerializeField] private GameObject cartelUI;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (datos == null)
        {
            Debug.LogError("ObjetoRecogible sin DatosItem asignado.", this);
            return;
        }

        if (jugador.mochila == null)
        {
            Debug.LogError("ControladorPersonaje no tiene la mochila asignada.", jugador);
            return;
        }

        if (!jugador.mochila.TryAgregar(datos))
        {
            Debug.Log($"Mochila llena: no se puede recoger '{datos.nombre}'.");
            return;
        }

        MostrarPista(false);
        Destroy(gameObject);
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelUI != null) cartelUI.SetActive(mostrar);
    }
}