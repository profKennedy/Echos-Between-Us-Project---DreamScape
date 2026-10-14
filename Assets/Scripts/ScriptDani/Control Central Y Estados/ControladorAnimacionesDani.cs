using UnityEngine;

public class ControladorAnimacionesDani : MonoBehaviour
{
    private Animator _animator;
    private ControladorPersonaje _controlador;
    private MovimientoPersonaje _movimiento;
    private InteraccionPersonaje _interaccion;

    [SerializeField] private AudioSource audioSourcePasos;
    [SerializeField] private AudioSource audioSourceLatidos;
    [SerializeField] private AudioSource audioSourceSfx;

    [SerializeField] private AudioClip sigilo;
    [SerializeField] private AudioClip corrida;
    [SerializeField] private AudioClip salto;
    [SerializeField] private AudioClip latido;

    private const float UmbralMovimiento = 0.1f;

    // Hashes son más performantes que strings
    private static readonly int EstaCorriendo = Animator.StringToHash("EstaCorriendo");
    private static readonly int EnSigilo = Animator.StringToHash("EnSigilo");
    private static readonly int EstaSaltando = Animator.StringToHash("EstaSaltando");
    private static readonly int EstaAsustada = Animator.StringToHash("EstaAsustada");
    private static readonly int Recoger = Animator.StringToHash("Recoger");

    private bool _estabaEnSuelo = true;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _controlador = GetComponent<ControladorPersonaje>();
        _movimiento = GetComponent<MovimientoPersonaje>();
        _interaccion = GetComponent<InteraccionPersonaje>();

        if (audioSourcePasos == null) audioSourcePasos = GetComponent<AudioSource>();
        if (audioSourceSfx == null) audioSourceSfx = audioSourcePasos;
    }

    private void OnEnable()
    {
        _controlador.OnEstadoCambiado += AlCambiarEstado;
        _interaccion.OnLevantarObjeto += ReproducirAnimacionRecoger;
    }

    private void OnDisable()
    {
        _controlador.OnEstadoCambiado -= AlCambiarEstado;
        _interaccion.OnLevantarObjeto -= ReproducirAnimacionRecoger;
    }

    private void Update()
    {
        float velocidadHorizontal = new Vector2(
            _movimiento.rb.velocity.x,
            _movimiento.rb.velocity.z
        ).magnitude;

        bool enMovimiento = velocidadHorizontal > UmbralMovimiento;

        // UNA sola consulta de suelo por frame (antes eran dos SphereCast), reutilizada abajo
        bool enSuelo = _movimiento.EstaEnSuelo();

        _animator.SetInteger(EstaCorriendo, enMovimiento ? 1 : 0);
        _animator.SetBool(EstaSaltando, !enSuelo);

        ActualizarAudioPasos(enMovimiento, enSuelo);

        if (_estabaEnSuelo && !enSuelo && audioSourceSfx != null && salto != null)
            audioSourceSfx.PlayOneShot(salto);

        _estabaEnSuelo = enSuelo;
    }

    private void ActualizarAudioPasos(bool enMovimiento, bool enSuelo)
    {
        if (audioSourcePasos == null) return;

        if (enMovimiento && enSuelo)
        {
            AudioClip clipActual = _animator.GetBool(EnSigilo) ? sigilo : corrida;

            if (audioSourcePasos.clip != clipActual || !audioSourcePasos.isPlaying)
            {
                audioSourcePasos.clip = clipActual;
                audioSourcePasos.loop = true;
                audioSourcePasos.Play();
            }
        }
        else if (audioSourcePasos.isPlaying)
        {
            audioSourcePasos.Stop();
        }
    }

    private void ReproducirAnimacionRecoger()
    {
        _animator.SetTrigger(Recoger);
    }

    private void AlCambiarEstado(EstadoPersonaje nuevoEstado)
    {
        // Resetear todo primero
        _animator.SetBool(EnSigilo, false);
        _animator.SetBool(EstaAsustada, false);

        if (audioSourceLatidos != null && audioSourceLatidos.isPlaying) audioSourceLatidos.Stop();

        switch (nuevoEstado)
        {
            case EstadoPersonaje.SIGILO:
                _animator.SetBool(EnSigilo, true);
                break;

            case EstadoPersonaje.ASUSTADA:
                _animator.SetBool(EstaAsustada, true);

                if (audioSourceLatidos != null && latido != null)
                {
                    audioSourceLatidos.clip = latido;
                    audioSourceLatidos.loop = true;
                    audioSourceLatidos.Play();
                }
                break;

            case EstadoPersonaje.LIBRE:
            case EstadoPersonaje.EN_FLASHBACK:
                // Libre y Flashback no tienen parámetro propio,
                // el bloqueo de movimiento en FLASHBACK lo maneja MovimientoPersonaje
                break;
        }
    }
}