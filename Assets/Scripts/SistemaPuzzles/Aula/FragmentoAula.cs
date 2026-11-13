
using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using UnityEngine;

public class FragmentoAula : MonoBehaviour, IObjetoLevantable
{
    [Header("Fragmento")]
    [SerializeField] private DatosItem datos;
    [SerializeField] private GameObject cartelUI;

    [Header("Puzzle del aula")]
    [SerializeField] private EventoEncogimientoAula eventoAula;

    private bool _recogido = false;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (_recogido)
            return;

        if (datos == null)
        {
            Debug.LogError("FragmentoAula sin DatosItem asignado.", this);
            return;
        }

        if (jugador.mochila == null)
        {
            Debug.LogError("Dani no tiene mochila asignada.", this);
            return;
        }

        if (eventoAula == null)
        {
            Debug.LogError("FragmentoAula sin EventoEncogimientoAula asignado.", this);
            return;
        }

        // Primero comprobamos que el fragmento se pueda guardar.
        if (!jugador.mochila.TryAgregar(datos))
        {
            Debug.Log("Mochila llena. No se pudo recoger el fragmento.");
            return;
        }

        _recogido = true;

        MostrarPista(false);

        // Avisamos que se completó el objetivo del aula.
        eventoAula.RegistrarFragmento();

        Debug.Log("Fragmento del aula recogido.");

        Destroy(gameObject);
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelUI != null)
            cartelUI.SetActive(mostrar && !_recogido);
    }
}
