using UnityEngine;

// Se agrega solo a los objetos que Dani suelta de la mochila (lo hace MochilaDani).
// Red de seguridad: una llave perdida bajo el piso dejaría el juego sin salida,
// así que si el objeto cae muy por debajo de donde apareció, vuelve a ese lugar.
[RequireComponent(typeof(Rigidbody))]
public class ObjetoSoltado : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3 _posicionSegura;
    private float _alturaLimite;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Configurar(Vector3 posicionSegura, float caidaMaxima)
    {
        _posicionSegura = posicionSegura;
        _alturaLimite = posicionSegura.y - caidaMaxima;
    }

    private void FixedUpdate()
    {
        if (_rb.position.y >= _alturaLimite) return;

        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.position = _posicionSegura;
        transform.position = _posicionSegura;

        Debug.LogWarning($"ObjetoSoltado: '{name}' atravesó el piso y se devolvió a su lugar.", this);
    }
}