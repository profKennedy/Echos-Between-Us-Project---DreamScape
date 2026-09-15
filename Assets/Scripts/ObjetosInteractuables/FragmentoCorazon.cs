using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentoCorazon : MonoBehaviour,IInteractable
{
    public GameObject cartelUI;
    public void Interactuar(ControladorPersonaje jugador)
    {
        Debug.Log("¡Fragmento del corazón recogido! Fin del nivel.");
        gameObject.SetActive(false);

        // ponytail: llamar al gestor de escena cuando esté implementado
        // GestorJuego.Instancia.TerminarNivel();
    }
    public void MostrarPista(bool mostrar)
    {
        cartelUI.SetActive(mostrar);
    }
}
