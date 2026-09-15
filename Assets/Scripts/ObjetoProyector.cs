using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjetoProyector : MonoBehaviour
{
    public bool generaPlataforma = true;
    public bool generaCamino = false;

    // La plataforma física que aparecerá
    public PlataformaSombra sombraGenerada;

    public void AlIluminar()
    {
        if (sombraGenerada != null)
        {
            // Posicionar la sombraGenerada matemáticamente basándose en el ángulo de la luz
            sombraGenerada.Activar();
        }
    }

    public void AlDejarDeIluminar()
    {
        if (sombraGenerada != null)
        {
            sombraGenerada.Desactivar();
        }
    }
}
