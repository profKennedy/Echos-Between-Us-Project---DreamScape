using System;
using UnityEngine;

// El "ente" que persigue a Dani (por ahora un cubo negro). Avanza en línea recta a velocidad fija
// desde el punto de aparición hasta el punto de detención (donde se traba).
// Solo se mueve y avisa si toca a Dani: quién decide qué pasa después es PersecucionNiebla.
// Objeto: Cubo con BoxCollider + Rigidbody (Is Kinematic, sin gravedad). Arranca DESACTIVADO.
//
// La detección NO usa OnTriggerEnter: pregunta cada paso de física "¿hay algo de Dani dentro de mi caja?".
// Así no depende de la matriz de colisiones de capas, ni de que Dani ya estuviera dentro al aparecer.
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class NieblaPerseguidora : MonoBehaviour
{
    [Tooltip("Tiene que ser menor que la velocidad de Dani (5 por defecto, 2.5 en sigilo) o no hay forma de perderla.")]
    [SerializeField] private float velocidad = 3.8f;
    [Tooltip("Pausa desde que aparece hasta que arranca: le da a Dani un instante para reaccionar.")]
    [SerializeField] private float segundosAntesDeArrancar = 0.75f;

    public event Action OnAlcanzoAJugador;
    public event Action OnDetenida;

    private Rigidbody _rb;
    private BoxCollider _caja;
    private readonly Collider[] _hits = new Collider[16];

    private Vector3 _direccion;
    private float _distanciaRestante;
    private float _espera;
    private bool _moviendo;
    private bool _letal;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _caja = GetComponent<BoxCollider>();
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

    // Dani escapó: la niebla sigue hasta el punto de detención, pero ya no puede hacerle daño
    public void HacerInofensiva()
    {
        _letal = false;
    }

    private void FixedUpdate()
    {
        Avanzar();

        if (_letal) BuscarJugador();
    }

    private void Avanzar()
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
            // Llegó al final del recorrido: es demasiado grande para seguir, se queda ahí
            _moviendo = false;
            _letal = false;
            OnDetenida?.Invoke();
        }
    }

    private void BuscarJugador()
    {
        Vector3 centro = transform.TransformPoint(_caja.center);
        Vector3 mitad = Vector3.Scale(_caja.size, transform.lossyScale) * 0.5f;
        mitad = new Vector3(Mathf.Abs(mitad.x), Mathf.Abs(mitad.y), Mathf.Abs(mitad.z));

        int cantidad = Physics.OverlapBoxNonAlloc(centro, mitad, _hits, transform.rotation, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < cantidad; i++)
        {
            // Dani puede tener varios colliders: se busca el controlador en el objeto o sus padres
            if (_hits[i].GetComponentInParent<ControladorPersonaje>() == null) continue;

            _letal = false; // un solo aviso por intento
            Debug.Log("NieblaPerseguidora: alcanzó a Dani.", this);
            OnAlcanzoAJugador?.Invoke();
            return;
        }
    }
}