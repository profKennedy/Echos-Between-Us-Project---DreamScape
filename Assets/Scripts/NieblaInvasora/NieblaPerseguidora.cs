using System;
using UnityEngine;

// El "ente" que persigue a Dani (por ahora un cubo negro). Avanza en línea recta a velocidad fija
// desde el punto de aparición hasta el punto de detención (el marco de la puerta, donde se traba).
// Solo se mueve y avisa si toca a Dani: quién decide qué pasa después es PersecucionNiebla.
// Objeto: Cubo con BoxCollider (Is Trigger) + Rigidbody (Is Kinematic, sin gravedad). Arranca DESACTIVADO.
[RequireComponent(typeof(Rigidbody))]
public class NieblaPerseguidora : MonoBehaviour
{
    [Tooltip("Tiene que ser menor que la velocidad de Dani (5 por defecto, 2.5 en sigilo) o no hay forma de perderla.")]
    [SerializeField] private float velocidad = 3.8f;
    [Tooltip("Pausa desde que aparece hasta que arranca: le da a Dani un instante para reaccionar.")]
    [SerializeField] private float segundosAntesDeArrancar = 0.75f;

    public event Action OnAlcanzoAJugador;
    public event Action OnDetenida;

    private Rigidbody _rb;
    private Vector3 _direccion;
    private float _distanciaRestante;
    private float _espera;
    private bool _moviendo;
    private bool _letal;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Iniciar(Vector3 origen, Vector3 destino)
    {
        Vector3 recorrido = destino - origen;
        recorrido.y = 0f; // avanza siempre a la altura del punto de aparición

        _direccion = recorrido.normalized;
        _distanciaRestante = recorrido.magnitude;
        _espera = segundosAntesDeArrancar;
        _moviendo = _distanciaRestante > 0.01f;
        _letal = true;

        // Se coloca por Rigidbody Y por Transform: así el cambio es inmediato, sin esperar al paso de física
        _rb.position = origen;
        transform.position = origen;
    }

    // Dani cayó: la niebla se congela donde está y deja de ser peligrosa
    public void Detener()
    {
        _moviendo = false;
        _letal = false;
    }

    // Dani escapó: la niebla sigue hasta el marco de la puerta, pero ya no puede hacerle daño
    public void HacerInofensiva()
    {
        _letal = false;
    }

    private void FixedUpdate()
    {
        if (!_moviendo) return;

        if (_espera > 0f)
        {
            _espera -= Time.fixedDeltaTime;
            return;
        }

        float paso = Mathf.Min(velocidad * Time.fixedDeltaTime, _distanciaRestante);
        _rb.MovePosition(_rb.position + _direccion * paso);
        _distanciaRestante -= paso;

        if (_distanciaRestante <= 0f)
        {
            // Llegó al marco de la puerta: es demasiado grande para pasar, se queda ahí
            _moviendo = false;
            _letal = false;
            OnDetenida?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other) => Comprobar(other);
    private void OnTriggerStay(Collider other) => Comprobar(other);

    private void Comprobar(Collider other)
    {
        if (!_letal) return;

        // Dani puede tener varios colliders: se busca el controlador en el objeto o sus padres
        if (other.GetComponentInParent<ControladorPersonaje>() != null)
        {
            _letal = false; // un solo aviso por intento
            OnAlcanzoAJugador?.Invoke();
        }
    }
}