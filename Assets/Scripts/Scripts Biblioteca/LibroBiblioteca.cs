using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using UnityEngine;

// Libro del puzzle de la biblioteca. Dani lo levanta con E (con la animación de recoger)
// y avisa al EncierroBiblioteca. El libro necesita un Collider y la layer "Interactuable".
public class LibroBiblioteca : MonoBehaviour, IObjetoLevantable
{
    [SerializeField] private EncierroBiblioteca encierro;
    [SerializeField] private GameObject cartelUI;

    private bool _recogido;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (_recogido) return;
        _recogido = true;

        MostrarPista(false);

        if (encierro != null) encierro.RegistrarLibro();
        else Debug.LogError("LibroBiblioteca sin EncierroBiblioteca asignado.", this);

        Destroy(gameObject);
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelUI != null) cartelUI.SetActive(mostrar && !_recogido);
    }
}