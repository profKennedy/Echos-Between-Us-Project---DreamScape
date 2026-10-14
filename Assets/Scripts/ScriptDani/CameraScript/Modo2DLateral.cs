using UnityEngine;

public class Modo2DLateral : IModoMovimiento
{
    private readonly Vector3 _ejeLateral;

    // Por defecto el eje X global. Con un riel de cámara podés pasar otro eje.
    public Modo2DLateral() : this(Vector3.right) { }

    public Modo2DLateral(Vector3 ejeLateral)
    {
        _ejeLateral = ejeLateral.normalized;
    }

    public Vector3 CalcularDireccion(Vector2 entradaDir)
    {
        // Solo cuenta la componente horizontal del input; la vertical se ignora
        if (Mathf.Abs(entradaDir.x) < 0.01f) return Vector3.zero;
        return _ejeLateral * Mathf.Sign(entradaDir.x);
    }
}