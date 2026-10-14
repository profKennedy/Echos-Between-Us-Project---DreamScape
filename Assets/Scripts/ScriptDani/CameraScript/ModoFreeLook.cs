using UnityEngine;

public class ModoFreeLook : IModoMovimiento
{
    private readonly Transform _camaraTransform;

    public ModoFreeLook(Transform camara) => _camaraTransform = camara;

    public Vector3 CalcularDireccion(Vector2 entradaDir)
    {
        // Ángulo del input + giro horizontal (yaw) de la cámara = dirección relativa a lo que ve el jugador.
        // Devuelve un vector unitario, así las diagonales no son más rápidas.
        float angulo = Mathf.Atan2(entradaDir.x, entradaDir.y) * Mathf.Rad2Deg + _camaraTransform.eulerAngles.y;
        return Quaternion.Euler(0f, angulo, 0f) * Vector3.forward;
    }
}