using UnityEngine;
using UnityEngine.Events;

// Cuenta los reflectores encendidos (en cualquier orden). Con el último, abre la puerta.
public class PuzzleReflectores : MonoBehaviour
{
    [SerializeField] private ReflectorEscenario[] reflectores;
    [Tooltip("La puerta del pasillo. Tiene que tener marcado 'Bloqueada Por Mecanismo'.")]
    [SerializeField] private Puerta puerta;
    [Tooltip("Opcional: sonido, un mensaje, un cambio de música...")]
    [SerializeField] private UnityEvent alCompletar;

    private bool _completado;

    private void OnEnable()
    {
        foreach (ReflectorEscenario r in reflectores)
            if (r != null) r.OnEncendido += AlEncenderseUno;
    }

    private void OnDisable()
    {
        foreach (ReflectorEscenario r in reflectores)
            if (r != null) r.OnEncendido -= AlEncenderseUno;
    }

    private void AlEncenderseUno(ReflectorEscenario _)
    {
        if (_completado) return;

        int encendidos = 0;
        foreach (ReflectorEscenario r in reflectores)
            if (r != null && r.EstaEncendido) encendidos++;

        Debug.Log($"Reflectores: {encendidos}/{reflectores.Length}");

        if (encendidos < reflectores.Length) return;

        _completado = true;
        if (puerta != null) puerta.AbrirPorMecanismo();
        alCompletar?.Invoke();
    }
}