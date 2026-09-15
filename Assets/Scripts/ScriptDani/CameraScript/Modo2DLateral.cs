using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modo2DLateral : IModoMovimiento
{
    public Vector3 CalcularDireccion(Vector2 entradaDir)
    {
        // Solo eje X global; la Y de entrada se ignora
        return new Vector3(entradaDir.x, 0f, 0f);
    }
}
