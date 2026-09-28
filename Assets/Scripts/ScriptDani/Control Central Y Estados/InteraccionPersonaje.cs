using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteraccionPersonaje : MonoBehaviour
{
    public float radioDeteccion = 2f;
    public LayerMask layerInteractuable;

    private IInteractable _objetoCercanoActual;

    // Objeto que estamos esperando recoger por animacion
    private IInteractable _interaccionPendiente;
    private ControladorPersonaje _jugador;

    public event Action OnLevantarObjeto;
    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radioDeteccion, layerInteractuable);

        IInteractable nuevo = hits.Length > 0 && hits[0].TryGetComponent(out IInteractable t) ? t : null;

        // Si cambió el objeto cercano, actualizás los carteles
        if (nuevo != _objetoCercanoActual)
        {
            _objetoCercanoActual?.MostrarPista(false); // oculta el anterior
            nuevo?.MostrarPista(true);                 // muestra el nuevo
            _objetoCercanoActual = nuevo;
        }
    }

    public void IntentarInteractuar()
    {
        if (_objetoCercanoActual == null)
            return;

        _jugador = GetComponent<ControladorPersonaje>();

        // Si es un objeto que se levanta,
        // primero hacemos la animación.
        if (_objetoCercanoActual is IObjetoLevantable)
        {
            _interaccionPendiente = _objetoCercanoActual;

            OnLevantarObjeto?.Invoke();

            return;
        }

        // Cualquier otra interacción se ejecuta normalmente.
        _objetoCercanoActual.Interactuar(_jugador);
    }
    public void CompletarLevantamiento()
    {
        if (_interaccionPendiente == null) return;
        _interaccionPendiente.Interactuar(_jugador);
        _interaccionPendiente = null;
    }

    public void PerderLinternaPorSusto() { } // ponytail: lógica omitida hasta linkear módulo Linterna.
    public bool EsLinternaCercana() => false;
}
