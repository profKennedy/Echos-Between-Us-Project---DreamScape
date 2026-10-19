using System;
using System.Collections;
using UnityEngine;

public class NivelDeAutoestima : MonoBehaviour
{
    [Header("Valores")]
    public float autoestimaMaxima = 100f;
    public float autoestimaActual;

    [Header("Checkpoints")]
    [Tooltip("Dónde reaparece Dani. Poné un Empty en el inicio del nivel como primer checkpoint; los fantasmas lo reemplazan al activarse.")]
    public Transform ultimoCheckpoint;
    [Tooltip("Cuánto tiempo queda derrotada antes de reaparecer.")]
    [SerializeField] private float segundosDerrotada = 2f;

    public event Action<float> OnAutoestimaCambiada;
    public event Action OnConsumidaPorNiebla;

    // Se dispara cuando cambia el checkpoint activo (los fantasmas lo usan para prender/apagar su indicador)
    public event Action<Transform> OnCheckpointCambiado;

    // Se dispara cuando Dani ya reapareció y puede moverse. Lleva el checkpoint donde reapareció
    // (el fantasma de ese checkpoint reconoce que es el suyo y va a hablarle).
    public event Action<Transform> OnReaparecida;

    private ControladorPersonaje _controladorDani;
    private bool _estaDerrotada = false;

    private void Awake()
    {
        _controladorDani = GetComponent<ControladorPersonaje>();
        autoestimaActual = autoestimaMaxima;
    }

    private void Start()
    {
        OnAutoestimaCambiada?.Invoke(ObtenerPorcentaje());
    }

    public void ReducirAutoestima(float cantidad)
    {
        if (_estaDerrotada) return;

        autoestimaActual -= cantidad;
        if (autoestimaActual <= 0f)
        {
            autoestimaActual = 0f;
            _estaDerrotada = true;
            ConsumidaPorNiebla();
        }
        OnAutoestimaCambiada?.Invoke(ObtenerPorcentaje());
    }

    public void RecuperarAutoestima(float cantidad)
    {
        if (_estaDerrotada) return;

        autoestimaActual += cantidad;
        if (autoestimaActual > autoestimaMaxima)
        {
            autoestimaActual = autoestimaMaxima;
        }
        OnAutoestimaCambiada?.Invoke(ObtenerPorcentaje());
    }

    private void ConsumidaPorNiebla()
    {
        OnConsumidaPorNiebla?.Invoke();

        // Bloqueamos movimiento mientras hacemos el respawn
        _controladorDani.movimiento.Bloquear(true);
        StartCoroutine(RutinaRespawn());
    }

    private IEnumerator RutinaRespawn()
    {
        yield return new WaitForSeconds(segundosDerrotada); // Tiempo que Dani queda derrotada

        if (ultimoCheckpoint != null)
        {
            TeletransportarA(ultimoCheckpoint);
        }
        else
        {
            Debug.LogWarning("no hay checkpoint asignado, se reinicia en la posicion actual");
        }

        autoestimaActual = autoestimaMaxima;
        _estaDerrotada = false;
        OnAutoestimaCambiada?.Invoke(ObtenerPorcentaje());

        _controladorDani.CambiarEstado(EstadoPersonaje.LIBRE);
        _controladorDani.movimiento.Bloquear(false);

        // Al final: Dani ya está lista, ahora el fantasma de ese checkpoint puede acercarse
        OnReaparecida?.Invoke(ultimoCheckpoint);
    }

    // Dani tiene Rigidbody: mover solo el transform deja la física con la posición vieja.
    private void TeletransportarA(Transform destino)
    {
        Rigidbody rb = _controladorDani.movimiento.rb;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = destino.position;
            rb.rotation = destino.rotation;
        }

        transform.SetPositionAndRotation(destino.position, destino.rotation);
    }

    public float ObtenerPorcentaje() => autoestimaActual / autoestimaMaxima;

    public void SetCheckpoint(Transform nuevoCheckpoint)
    {
        if (ultimoCheckpoint == nuevoCheckpoint) return;

        ultimoCheckpoint = nuevoCheckpoint;
        OnCheckpointCambiado?.Invoke(ultimoCheckpoint);
    }
}