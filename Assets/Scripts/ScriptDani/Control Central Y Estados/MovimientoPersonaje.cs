using UnityEngine;
using System;

public class MovimientoPersonaje : MonoBehaviour
{
    public Rigidbody rb;
    public float velocidadBase = 5f;
    public float velocidadActual = 5f;
    public float velocidadRotacion = 10f;
    public float fuerzaSaltoBase = 6f;
    public float fuerzaSaltoActual = 6f;
    public bool enSigilo;
    public LayerMask layerSuelo;
    public event Action OnSaltoRealizado;

    public IModoMovimiento modoActual;

    private const float MultiplicadorSigilo = 0.5f;

    private Vector2 _inputMovimiento;
    private bool _bloqueado = false;
    private float _factorEncogimiento = 1f;
    private CapsuleCollider _capsule;

    private void Awake()
    {
        _capsule = GetComponent<CapsuleCollider>();
    }

    public void Mover(Vector2 dir)
    {
        _inputMovimiento = dir;
    }

    private void FixedUpdate()
    {
        if (_bloqueado) return;

        // El MODO decide la dirección (3D relativa a cámara, 2D lateral, etc.).
        // MovimientoPersonaje no necesita saber cuál es: eso es el patrón Strategy.
        Vector3 direccion = Vector3.zero;
        if (_inputMovimiento.sqrMagnitude > 0.01f && modoActual != null)
        {
            direccion = modoActual.CalcularDireccion(_inputMovimiento);
            direccion.y = 0f;
        }

        // Sin input, o sin dirección válida (ej: solo input vertical en modo 2D): frenar
        if (direccion.sqrMagnitude < 0.0001f)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        direccion.Normalize();

        rb.velocity = new Vector3(
            direccion.x * velocidadActual,
            rb.velocity.y, // Respetamos la gravedad
            direccion.z * velocidadActual
        );

        Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, rotacionDeseada, Time.fixedDeltaTime * velocidadRotacion));
    }

    public void Bloquear(bool valor)
    {
        _bloqueado = valor;
        // Si bloqueamos, frenamos: si no, el Rigidbody sigue deslizando con la última velocidad
        if (valor) rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    public void Saltar()
    {
        if (_bloqueado) return;

        if (EstaEnSuelo())
        {
            rb.AddForce(
                Vector3.up * fuerzaSaltoActual,
                ForceMode.Impulse
            );

            OnSaltoRealizado?.Invoke();
        }
    }

    public void ActivarSigilo(bool activo)
    {
        enSigilo = activo;
        ActualizarVelocidad();
    }

    public void AplicarModificadoresEncogimiento(float factor)
    {
        _factorEncogimiento = factor;
        ActualizarVelocidad();
    }

    // UN solo lugar calcula la velocidad.
    // Regla: el sigilo es el TOPE. Si el encogimiento ya la dejó más lenta que el sigilo, se queda esa.
    private void ActualizarVelocidad()
    {
        float factor = enSigilo
            ? Mathf.Min(_factorEncogimiento, MultiplicadorSigilo)
            : _factorEncogimiento;

        velocidadActual = velocidadBase * factor;
    }

    public bool EstaEnSuelo()
    {
        Vector3 origen = _capsule.bounds.center;
        float radio = _capsule.radius * 0.9f;
        float distancia = _capsule.bounds.extents.y + 0.1f;
        return Physics.SphereCast(origen, radio, Vector3.down, out _, distancia, layerSuelo);
    }
}