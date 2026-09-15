using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaSombra : MonoBehaviour
{
    public bool esTransitable = true;
    public float duracion = 0f; // Si es 0, no desaparece mientras haya luz

    // Referencias a los componentes de Unity
    private Collider colisionador;
    private MeshRenderer renderizador;

    private void Awake()
    {
        colisionador = GetComponent<Collider>();
        renderizador = GetComponent<MeshRenderer>();
        Desactivar(); // Empiezan desactivadas por defecto
    }

    public void Activar()
    {
        if (esTransitable && colisionador != null) colisionador.enabled = true;
        if (renderizador != null) renderizador.enabled = true;
    }

    public void Desactivar()
    {
        if (colisionador != null) colisionador.enabled = false;
        if (renderizador != null) renderizador.enabled = false;
    }
}
