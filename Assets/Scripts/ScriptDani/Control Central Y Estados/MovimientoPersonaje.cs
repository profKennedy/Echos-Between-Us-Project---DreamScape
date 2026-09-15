using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public IModoMovimiento modoActual;

    private Vector2 _inputMovimiento;
    private bool _bloqueado = false;
    private CapsuleCollider _capsule;
    private Transform _camaraTransform;

    private void Awake()
    {
        _capsule = GetComponent<CapsuleCollider>();
        _camaraTransform = Camera.main.transform; // Referencia directa a la cámara
    }

    public void Mover(Vector2 dir)
    {
        _inputMovimiento = dir;
    }

    private void FixedUpdate()
    {
        if (_bloqueado) return;

        // Verificamos si hay movimiento significativo
        if (_inputMovimiento.sqrMagnitude > 0.01f)
        {
            Vector3 direccionMovimiento = Vector3.zero;

            // Verificamos si estamos en tercera persona
            if (modoActual is ModoFreeLook)
            {
                // LA MAGIA MATEMÁTICA ABSOLUTA:
                // Mathf.Atan2 nos da el ángulo de tu joystick/teclado.
                // Al sumarle _camaraTransform.eulerAngles.y, ignoramos por completo el balanceo y el lag de Cinemachine.
                float anguloObjetivo = Mathf.Atan2(_inputMovimiento.x, _inputMovimiento.y) * Mathf.Rad2Deg + _camaraTransform.eulerAngles.y;

                // Convertimos ese ángulo perfecto en una dirección 3D plana
                direccionMovimiento = Quaternion.Euler(0f, anguloObjetivo, 0f) * Vector3.forward;
            }
            else if (modoActual != null)
            {
                // Modo 2D u otros: usa tu interfaz clásica pero aplanando la Y
                Vector3 dirCamara = modoActual.CalcularDireccion(_inputMovimiento);
                direccionMovimiento = new Vector3(dirCamara.x, 0f, dirCamara.z).normalized;
            }

            // 1. Aplicamos la velocidad limpia
            rb.velocity = new Vector3(
                direccionMovimiento.x * velocidadActual,
                rb.velocity.y, // Respetamos la gravedad
                direccionMovimiento.z * velocidadActual
            );

            // 2. Rotamos a Dany hacia la dirección del movimiento
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionMovimiento);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rotacionDeseada, Time.fixedDeltaTime * velocidadRotacion));
        }
        else
        {
            // Frena al personaje inmediatamente sin deslizarse por el piso
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    public void Bloquear(bool valor) => _bloqueado = valor;

    public void Saltar() { if (EstaEnSuelo()) rb.AddForce(Vector3.up * fuerzaSaltoActual, ForceMode.Impulse); }

    public void ActivarSigilo(bool activo)
    {
        enSigilo = activo;
        velocidadActual = activo ? velocidadBase * 0.5f : velocidadBase;
    }

    public void AplicarModificadoresEncogimiento(float factor) => velocidadActual = velocidadBase * factor;

    public bool EstaEnSuelo()
    {
        Vector3 origen = _capsule.bounds.center;
        float radio = _capsule.radius * 0.9f;
        float distancia = _capsule.bounds.extents.y + 0.1f;
        return Physics.SphereCast(origen, radio, Vector3.down, out _, distancia, layerSuelo);
    }
}