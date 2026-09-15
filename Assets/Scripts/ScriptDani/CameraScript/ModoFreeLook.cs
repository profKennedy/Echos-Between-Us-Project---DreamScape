using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModoFreeLook : IModoMovimiento
{
    private readonly Transform camaraTransform;

    public ModoFreeLook(Transform camara) => camaraTransform = camara;

    public Vector3 CalcularDireccion(Vector2 entradaDir)
    {
        // Forward y Right de la cámara proyectados sobre el plano horizontal
        Vector3 forward = Vector3.ProjectOnPlane(camaraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(camaraTransform.right, Vector3.up).normalized;
        return (forward * entradaDir.y + right * entradaDir.x);
    }
}
