using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System;
using UnityEngine;
using System.Collections;

public class InteraccionPersonaje : MonoBehaviour
{
    public float radioDeteccion = 2f;
    public LayerMask layerInteractuable;
    private ControladorAnimacionesDani _animaciones;
    private MovimientoPersonaje _movimiento;
    private bool _recogiendoObjeto = false;
    private bool _cinematicaActiva = false;
    [Header("Orientación al recoger objetos")]
    [SerializeField] private float velocidadGiroRecoger = 540f;

    // Buffer reutilizable: OverlapSphere (sin NonAlloc) crea un array nuevo cada frame
    private readonly Collider[] _hits = new Collider[8];

    private IInteractable _objetoCercanoActual;

    [Header("Seguridad")]
    [Tooltip("Si la animación de recoger no termina en este tiempo (se la interrumpió un susto, por ejemplo), se cancela sola y Dani vuelve a moverse.")]
    [SerializeField] private float tiempoMaximoRecoger = 4f;
    private Coroutine _vigilante;

    // Objeto que estamos esperando recoger por animacion
    private IInteractable _interaccionPendiente;
    private ControladorPersonaje _jugador;

    public event Action OnLevantarObjeto;

    private void Awake()
    {
        _jugador = GetComponent<ControladorPersonaje>();
        _movimiento = GetComponent<MovimientoPersonaje>();
        _animaciones = GetComponent<ControladorAnimacionesDani>();
    }

    private void OnEnable()
    {
        // Un susto interrumpe la animación de recoger (el Animator pasa a "Muerte" desde Any State)
        // y los Animation Events nunca llegan: hay que cancelar la recogida a mano.
        if (_jugador != null) _jugador.OnPersonajeAsustado += CancelarRecogida;
    }

    private void OnDisable()
    {
        if (_jugador != null) _jugador.OnPersonajeAsustado -= CancelarRecogida;
    }

    private void Update()
    {
        // No cambiar de objeto mientras se está recogiendo.
        if (_recogiendoObjeto)
            return;

        if (!EstaVivo(_objetoCercanoActual))
            _objetoCercanoActual = null;

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

        float tiempoTranscurrido = 0f;
        float tiempoMaximoGiro = 0.5f;

        while (Quaternion.Angle(rb.rotation, rotacionObjetivo) > 1f
               && tiempoTranscurrido < tiempoMaximoGiro)
        {
            Quaternion nuevaRotacion = Quaternion.RotateTowards(
                rb.rotation,
                rotacionObjetivo,
                velocidadGiroRecoger * Time.fixedDeltaTime
            );

            rb.MoveRotation(nuevaRotacion);

            tiempoTranscurrido += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        // No forzamos la rotación final si hay
        // una colisión que impide completar el giro.
    }


    // Un MonoBehaviour destruido sigue siendo != null para C# cuando lo mirás como interfaz.
    private static bool EstaVivo(IInteractable interactable)
    {
        if (interactable == null) return false;
        return !(interactable is UnityEngine.Object objeto) || objeto != null;
    }


    public void IntentarInteractuar()
    {

        // No permitir otra interacción mientras Dani recoge.
        if (_recogiendoObjeto || _cinematicaActiva)
            return;

        if (!EstaVivo(_objetoCercanoActual))
            return;

        // Si Dani está asustada, apretar interactuar corta el susto (igual que moverse);
        // si no, el trigger "Recoger" quedaría esperando dentro del estado de susto.
        if (_jugador != null && _jugador.estado == EstadoPersonaje.ASUSTADA)
            _jugador.CambiarEstado(EstadoPersonaje.LIBRE);

        // Si es un objeto que se levanta,
        // primero reproducimos la animación.
        if (_objetoCercanoActual is IObjetoLevantable)
        {
            // Bloqueamos nuevas interacciones inmediatamente.
            _recogiendoObjeto = true;

            // Detener y bloquear el movimiento.
            _movimiento.Bloquear(true);

            // Guardar el objeto pendiente.
            _interaccionPendiente = _objetoCercanoActual;

            // Red de seguridad: si la animación no llega a terminar, no queda trabado para siempre.
            _vigilante = StartCoroutine(VigilarRecogida());

            Component componenteObjeto =
                _objetoCercanoActual as Component;

            if (componenteObjeto != null)
            {
                StartCoroutine(
                    PrepararLevantamiento(componenteObjeto.transform)
                );
            }
            else
            {
                OnLevantarObjeto?.Invoke();
            }

            return;
        }

        // Las demás interacciones siguen funcionando.
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
        if (!_recogiendoObjeto)
            return;

        _recogiendoObjeto = false;

        if (_vigilante != null)
        {
            StopCoroutine(_vigilante);
            _vigilante = null;
        }

        if (_animaciones != null)
            _animaciones.LimpiarTriggerRecoger();

        // Solo devolver el movimiento si no hay
        // una cinemática en ejecución.
        if (!_cinematicaActiva)
        {
            _movimiento.Bloquear(false);
        }
    }

    // Corta una recogida a medias (susto o timeout) y deja todo como antes de apretar el botón.
    public void CancelarRecogida()
    {
        if (!_recogiendoObjeto)
            return;

        StopAllCoroutines();
        _vigilante = null;

        _recogiendoObjeto = false;
        _interaccionPendiente = null;

        if (_animaciones != null)
            _animaciones.LimpiarTriggerRecoger();

        if (!_cinematicaActiva)
            _movimiento.Bloquear(false);

        // Forzar que se vuelva a detectar el objeto cercano en el próximo Update
        _objetoCercanoActual?.MostrarPista(false);
        _objetoCercanoActual = null;
    }

    private IEnumerator VigilarRecogida()
    {
        yield return new WaitForSeconds(tiempoMaximoRecoger);

        if (_recogiendoObjeto)
        {
            Debug.LogWarning("InteraccionPersonaje: la animación de recoger no terminó a tiempo. Se cancela la recogida.", this);
            CancelarRecogida();
        }
    }

    public void PerderLinternaPorSusto() { } // ponytail: lógica omitida hasta linkear módulo Linterna.
    public bool EsLinternaCercana() => false;

    public void EstablecerBloqueoCinematica(bool bloquear)
    {
        _cinematicaActiva = bloquear;

        if (bloquear)
        {
            _movimiento.Bloquear(true);
        }
        else if (!_recogiendoObjeto)
        {
            _movimiento.Bloquear(false);
        }
    }
}