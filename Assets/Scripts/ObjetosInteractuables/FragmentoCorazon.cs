using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using UnityEngine;

public class FragmentoCorazon : MonoBehaviour, IObjetoLevantable
{
    // Mismo nombre que antes: la referencia del Inspector se conserva
    [SerializeField] private GameObject cartelUI;

    public void Interactuar(ControladorPersonaje jugador)
    {
        MostrarPista(false);
        gameObject.SetActive(false);

        // Un solo camino para terminar el nivel: el personaje entra en flashback,
        // se bloquea y carga la escena final.
        jugador.AlRecogerFragmento();
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelUI != null) cartelUI.SetActive(mostrar);
    }
}