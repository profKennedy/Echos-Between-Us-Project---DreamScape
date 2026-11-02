using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System;
using UnityEngine;
using System.Collections;

public class InteraccionPersonaje : MonoBehaviour
{
    public float radioDeteccion = 2f;
    public LayerMask layerInteractuable;
    private MovimientoPersonaje _movimiento;
    private bool _recogiendoObjeto = false;
    [Header("Orientación al recoger objetos")]
    [SerializeField] private float velocidadGiroRecoger = 540f;

    // Buffer reutilizable: OverlapSphere (sin NonAlloc) crea un array nuevo cada frame
    private readonly Collider[] _hits = new Collider[8];

    private IInteractable _objetoCercanoActual;

    // Objeto que estamos esperando recoger por animacion
    private IInteractable _interaccionPendiente;
    private ControladorPersonaje _jugador;

    public event Action OnLevantarObjeto;

    private void Awake()
    {
        _jugador = GetComponent<ControladorPersonaje>();
        _movimiento = GetComponent<MovimientoPersonaje>();
    }

    private void Update()
    {
        // Si el objeto fue destruido (Destroy), la referencia a la interfaz NO es null de C#
        if (!EstaVivo(_objetoCercanoActual)) _objetoCercanoActual = null;

        IInteractable nuevo = BuscarMasCercano();

        if (nuevo != _objetoCercanoActual)
        {
            _objetoCercanoActual?.MostrarPista(false);
            nuevo?.MostrarPista(true);
            _objetoCercanoActual = nuevo;
        }
    }

    private IInteractable BuscarMasCercano()
    {
        int cantidad = Physics.OverlapSphereNonAlloc(transform.position, radioDeteccion, _hits, layerInteractuable);

        IInteractable mejor = null;
        float mejorDistancia = float.MaxValue;

        for (int i = 0; i < cantidad; i++)
        {
            if (!_hits[i].TryGetComponent(out IInteractable candidato)) continue;

            float distancia = (_hits[i].transform.position - transform.position).sqrMagnitude;
            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = candidato;
            }
        }

        return mejor;
    }

    private IEnumerator PrepararLevantamiento(Transform objetivo)
    {
        // Primero orientar a Dany.
        yield return StartCoroutine(GirarHaciaObjeto(objetivo));

        // Después reproducir la animación de recoger.
        OnLevantarObjeto?.Invoke();
    }

    private IEnumerator GirarHaciaObjeto(Transform objetivo)
    {
        if (objetivo == null)
            yield break;

        Rigidbody rb = GetComponent<Rigidbody>();

        Vector3 direccion = objetivo.position - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.001f)
            yield break;

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

        while (Quaternion.Angle(rb.rotation, rotacionObjetivo) > 1f)
        {
            Quaternion nuevaRotacion = Quaternion.RotateTowards(
                rb.rotation,
                rotacionObjetivo,
                velocidadGiroRecoger * Time.fixedDeltaTime
            );

            rb.MoveRotation(nuevaRotacion);

            yield return new WaitForFixedUpdate();
        }

        rb.MoveRotation(rotacionObjetivo);
    }

    // Un MonoBehaviour destruido sigue siendo != null para C# cuando lo mirás como interfaz.
    private static bool EstaVivo(IInteractable interactable)
    {
        if (interactable == null) return false;
        return !(interactable is UnityEngine.Object objeto) || objeto != null;
    }

    public void IntentarInteractuar()
    {
        if (!EstaVivo(_objetoCercanoActual)) return;

        // Si es un objeto que se levanta, primero hacemos la animación.
        if (_objetoCercanoActual is IObjetoLevantable)
        {
            _recogiendoObjeto = true;

            // Detener y bloquear el movimiento.
            _movimiento.Bloquear(true);

            // Guardar el objeto pendiente.
            _interaccionPendiente = _objetoCercanoActual;

            // Obtener el Transform del objeto.
            // Esta conversión funciona si el objeto interactuable
            // es un componente MonoBehaviour.
            Component componenteObjeto = _objetoCercanoActual as Component;

            if (componenteObjeto != null)
            {
                StartCoroutine(PrepararLevantamiento(componenteObjeto.transform));
            }
            else
            {
                // Si no tiene Transform, reproducir la animación normalmente.
                OnLevantarObjeto?.Invoke();
            }

            return;
        }

        // Cualquier otra interacción se ejecuta normalmente.
        _objetoCercanoActual.Interactuar(_jugador);
    }

    // Se llama desde un Animation Event (ver nota sobre el Animator en un hijo)
    public void CompletarLevantamiento()
    {
        if (!_recogiendoObjeto) return;

        IInteractable pendiente = _interaccionPendiente;
        _interaccionPendiente = null;

        // Recoger el objeto en el momento indicado.
        if (EstaVivo(pendiente))
        {
            pendiente.Interactuar(_jugador);
        }

    }

    public void FinalizarAnimacionRecoger()
    {
        if (!_recogiendoObjeto) return;

        _recogiendoObjeto = false;

        // Dany puede volver a caminar y saltar.
        _movimiento.Bloquear(false);
    }

    public void PerderLinternaPorSusto() { } // ponytail: lógica omitida hasta linkear módulo Linterna.
    public bool EsLinternaCercana() => false;
}