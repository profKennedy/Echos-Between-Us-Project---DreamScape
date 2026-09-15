using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BateriaLinterna : MonoBehaviour,IInteractable
{
    public float cantidadRecarga = 50f;
    public GameObject cartelUI;
    public void Interactuar(ControladorPersonaje jugador)
    {
        // Busca la linterna en el jugador
        GestionLinterna gestion = jugador.gestionLinterna;

        if (gestion == null || gestion.linterna == null) return;
        if (gestion.LinternaDisponibleEnSuelo()) return; // no tiene la linterna en mano
        gestion.linterna.RecargarBateria(cantidadRecarga);
        Debug.Log($"Batería recargada en {cantidadRecarga}%");

        gameObject.SetActive(false); // desaparece al ser recogida
    }
    public void MostrarPista(bool mostrar)
    {
        cartelUI.SetActive(mostrar);
    }
}
