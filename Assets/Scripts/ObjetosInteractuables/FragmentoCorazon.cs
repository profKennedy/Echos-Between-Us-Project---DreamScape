using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentoCorazon : MonoBehaviour, IObjetoLevantable
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
