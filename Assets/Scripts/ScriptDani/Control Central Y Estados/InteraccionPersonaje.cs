using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteraccionPersonaje : MonoBehaviour
{
    public float radioDeteccion = 2f;
    public LayerMask layerInteractuable;


    private IInteractable _objetoCercanoActual;
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
        _objetoCercanoActual?.Interactuar(GetComponent<ControladorPersonaje>());
    }

    public void PerderLinternaPorSusto() { } // ponytail: lógica omitida hasta linkear módulo Linterna.
    public bool EsLinternaCercana() => false;
}
