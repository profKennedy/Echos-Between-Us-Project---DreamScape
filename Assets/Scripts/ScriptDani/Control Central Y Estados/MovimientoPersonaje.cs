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

    [Header("Movimiento al encogerse")]
    [SerializeField] private EfectoEncogimiento efectoEncogimiento;

    [SerializeField, Range(0.1f, 1f)]
    private float multiplicadorVelocidadMinima = 0.35f;

    [SerializeField, Range(0.1f, 1f)]
    private float multiplicadorSaltoMinimo = 0.45f;

    [Header("Suavizado de movimiento al encogerse")]
    [SerializeField] private float aceleracionEncogida = 8f;
    [SerializeField] private float desaceleracionEncogida = 12f;

    [Range(0f, 1f)]
    [SerializeField] private float controlEnAire = 0.6f;

    [Header("Gravedad al encogerse")]
    [SerializeField, Range(0.2f, 1f)]
    private float multiplicadorGravedadMinima = 0.55f;

    [Header("Control de pendientes")]
    [SerializeField] private float anguloMaximoPendiente = 45f;

    [SerializeField] private float distanciaDeteccionPendiente = 0.2f;

    private float gravedadOriginal;

    private void Awake()
    {

        _capsule = GetComponent<CapsuleCollider>();

        if (efectoEncogimiento == null)
            efectoEncogimiento = GetComponent<EfectoEncogimiento>();

        velocidadActual = velocidadBase;
        fuerzaSaltoActual = fuerzaSaltoBase;
        gravedadOriginal = Physics.gravity.y;
    }

    private void Update()
    {
        if (efectoEncogimiento == null)
            return;

        float factorActual = efectoEncogimiento.ObtenerFactorActual();

        float progreso = Mathf.InverseLerp(
            efectoEncogimiento.factorMinimo,
            1f,
            factorActual
        );

        float factorVelocidad = Mathf.Lerp(
            multiplicadorVelocidadMinima,
            1f,
            progreso
        );

        float factorSalto = Mathf.Lerp(
            multiplicadorSaltoMinimo,
            1f,
            progreso
        );

        AplicarModificadoresEncogimiento(factorVelocidad);

        fuerzaSaltoActual = fuerzaSaltoBase * factorSalto;

        float factorGravedad = Mathf.Lerp(
            multiplicadorGravedadMinima,
            1f,
            progreso
        );
    }

    public void Mover(Vector2 dir)
    {
        _inputMovimiento = dir;
    }

    private void FixedUpdate()
    {
        if (efectoEncogimiento != null && rb.useGravity)
        {
            float factorActual = efectoEncogimiento.ObtenerFactorActual();

            float progreso = Mathf.InverseLerp(
                efectoEncogimiento.factorMinimo,
                1f,
                factorActual
            );

            float factorGravedad = Mathf.Lerp(
                multiplicadorGravedadMinima,
                1f,
                progreso
            );

            // Compensamos parte de la gravedad normal.
            float compensacion = gravedadOriginal * (factorGravedad - 1f);

            rb.AddForce(
                Vector3.up * compensacion,
                ForceMode.Acceleration
            );
        }

        if (_bloqueado) return;

        // El MODO decide la dirección (3D relativa a cámara, 2D lateral, etc.).
        // MovimientoPersonaje no necesita saber cuál es: eso es el patrón Strategy.
        Vector3 direccion = Vector3.zero;
        if (_inputMovimiento.sqrMagnitude > 0.01f && modoActual != null)
        {
            direccion = modoActual.CalcularDireccion(_inputMovimiento);
            direccion.y = 0f;
        }


        bool hayMovimiento = direccion.sqrMagnitude > 0.0001f;

        if (hayMovimiento)
            direccion.Normalize();

        // Velocidad horizontal actual.
        Vector3 velocidadHorizontal = new Vector3(
            rb.velocity.x,
            0f,
            rb.velocity.z
        );

        // Velocidad horizontal deseada.
        Vector3 velocidadObjetivo = hayMovimiento
            ? direccion * velocidadActual
            : Vector3.zero;

        // Comprobamos si Dani está encogida.
        bool estaEncogida = efectoEncogimiento != null &&
                            efectoEncogimiento.EstaEncogida();

        if (estaEncogida)
        {
            // Aceleramos o frenamos progresivamente.
            float aceleracion = hayMovimiento
                ? aceleracionEncogida
                : desaceleracionEncogida;

            // En el aire puede corregir su trayectoria,
            // pero con menor aceleración.
            if (!EstaEnSuelo())
            {
                aceleracion *= controlEnAire;
            }

            velocidadHorizontal = Vector3.MoveTowards(
                velocidadHorizontal,
                velocidadObjetivo,
                aceleracion * Time.fixedDeltaTime
            );
        }
        else
        {
            // Dani normal conserva el movimiento original.
            velocidadHorizontal = velocidadObjetivo;
        }

        // Aplicamos el movimiento sin alterar la velocidad vertical.
        rb.velocity = new Vector3(
            velocidadHorizontal.x,
            rb.velocity.y,
            velocidadHorizontal.z
        );

        // Giramos solamente cuando hay dirección.
        if (hayMovimiento)
        {
            Quaternion rotacionDeseada =
                Quaternion.LookRotation(direccion);

            rb.MoveRotation(
                Quaternion.Slerp(
                    rb.rotation,
                    rotacionDeseada,
                    Time.fixedDeltaTime * velocidadRotacion
                )
            );
        }

        if (!hayMovimiento && DetectarPendiente(out RaycastHit pendiente))
        {
            // Evitamos que la gravedad la deslice por la pendiente.
            float factorGravedad = 1f;

            if (efectoEncogimiento != null)
            {
                float progreso = Mathf.InverseLerp(
                    efectoEncogimiento.factorMinimo,
                    1f,
                    efectoEncogimiento.ObtenerFactorActual()
                );

                factorGravedad = Mathf.Lerp(
                    multiplicadorGravedadMinima,
                    1f,
                    progreso
                );
            }

            Vector3 gravedad = Physics.gravity * factorGravedad;

            Vector3 gravedadParalela = Vector3.ProjectOnPlane(
                gravedad,
                pendiente.normal
            );

            rb.AddForce(
                -gravedadParalela,
                ForceMode.Acceleration
            );
        }

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


    private bool DetectarPendiente(out RaycastHit hit)
    {
        Vector3 origen = _capsule.bounds.center;

        float distancia = _capsule.bounds.extents.y
                          + distanciaDeteccionPendiente;

        if (Physics.Raycast(
            origen,
            Vector3.down,
            out hit,
            distancia,
            layerSuelo,
            QueryTriggerInteraction.Ignore))
        {
            float angulo = Vector3.Angle(
                hit.normal,
                Vector3.up
            );

            return angulo > 1f &&
                   angulo <= anguloMaximoPendiente;
        }

        return false;
    }

}