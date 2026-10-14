using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System;
using UnityEngine;

public class InteraccionPersonaje : MonoBehaviour
{
    public float radioDeteccion = 2f;
    public LayerMask layerInteractuable;

    // Buffer reutilizable: OverlapSphere (sin NonAlloc) crea un array nuevo cada frame
    private readonly Collider[] _hits = new Collider[8];

    private IInteractable _objetoCercanoActual;

    // Objeto que estamos esperando recoger por animacion
    private IInteractable _interaccionPendiente;
    private ControladorPersonaje _jugador;

    public event Action OnLevantarObjeto;

    private void Awake()
    {
        _jugador = GetComponent<ControladorPersonaje>();
    }

    private void Update()
    {
        // Si el objeto fue destruido (Destroy), la referencia a la interfaz NO es null de C#
        if (!EstaVivo(_objetoCercanoActual)) _objetoCercanoActual = null;

        IInteractable nuevo = BuscarMasCercano();

        if (nuevo != _objetoCercanoActual)
        {
            _objetoCercanoActual?.MostrarPista(false);
            nuevo?.MostrarPista(true);
            _objetoCercanoActual = nuevo;
        }
    }

    private IInteractable BuscarMasCercano()
    {
        int cantidad = Physics.OverlapSphereNonAlloc(transform.position, radioDeteccion, _hits, layerInteractuable);

        IInteractable mejor = null;
        float mejorDistancia = float.MaxValue;

        for (int i = 0; i < cantidad; i++)
        {
            if (!_hits[i].TryGetComponent(out IInteractable candidato)) continue;

            float distancia = (_hits[i].transform.position - transform.position).sqrMagnitude;
            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = candidato;
            }
        }

        return mejor;
    }

    // Un MonoBehaviour destruido sigue siendo != null para C# cuando lo mirás como interfaz.
    private static bool EstaVivo(IInteractable interactable)
    {
        if (interactable == null) return false;
        return !(interactable is UnityEngine.Object objeto) || objeto != null;
    }

    public void IntentarInteractuar()
    {
        if (!EstaVivo(_objetoCercanoActual)) return;

        // Si es un objeto que se levanta, primero hacemos la animación.
        if (_objetoCercanoActual is IObjetoLevantable)
        {
            _interaccionPendiente = _objetoCercanoActual;
            OnLevantarObjeto?.Invoke();
            return;
        }

        // Cualquier otra interacción se ejecuta normalmente.
        _objetoCercanoActual.Interactuar(_jugador);
    }

    // Se llama desde un Animation Event (ver nota sobre el Animator en un hijo)
    public void CompletarLevantamiento()
    {
        if (_interaccionPendiente == null) return;

        IInteractable pendiente = _interaccionPendiente;
        _interaccionPendiente = null; // limpiar ANTES de ejecutar

        if (EstaVivo(pendiente)) pendiente.Interactuar(_jugador);
    }

    public void PerderLinternaPorSusto() { } // ponytail: lógica omitida hasta linkear módulo Linterna.
    public bool EsLinternaCercana() => false;
}