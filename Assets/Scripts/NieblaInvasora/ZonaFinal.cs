using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using UnityEngine;
using UnityEngine.Events;

// El fragmento es la "llave" del final: esta zona solo dispara la cinemática si Dani lo lleva en la mochila.
// Va donde esté la maestra (o, por ahora, justo después de la puerta).
[RequireComponent(typeof(Collider))]
public class ZonaFinal : MonoBehaviour
{
    [Tooltip("El mismo asset DatosItem del fragmento.")]
    [SerializeField] private DatosItem itemRequerido;

    [Tooltip("Acá se enchufa la cinemática: PlayableDirector.Play, activar el canvas final, etc.")]
    [SerializeField] private UnityEvent alIniciarFinal;

    private bool _activada;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_activada) return;

        ControladorPersonaje jugador = other.GetComponentInParent<ControladorPersonaje>();
        if (jugador == null) return;

        if (jugador.mochila == null || !jugador.mochila.Contiene(itemRequerido))
        {
            Debug.Log("Dani todavía no tiene el fragmento.");
            return;
        }

        _activada = true;

        // Mismo estado que ya usabas para el final: bloquea sigilo y mochila
        jugador.CambiarEstado(EstadoPersonaje.EN_FLASHBACK);
        jugador.movimiento.Bloquear(true);

        alIniciarFinal?.Invoke();
    }
}