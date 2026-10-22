using UnityEngine;
using UnityEngine.Events;

public enum EstadoPersecucion
{
    Inactiva,    // todavía no empezó (o se reinició)
    EnCurso,     // Dani corre, la niebla avanza
    Fallida,     // cayó o la alcanzaron: esperando a reaparecer
    Completada   // escapó por la puerta
}

// Director de la persecución final. Es el ÚNICO que sabe el orden completo:
//   iniciar  -> cámara de riel + movimiento 2.5D + niebla + bloqueo de mochila/linterna
//   escapar  -> vuelve todo a la normalidad (la niebla se traba en la puerta)
//   fallar   -> Dani reaparece (NivelDeAutoestima) y acá se reinicia todo, incluido el fragmento
// Va en un objeto vacío de la escena (ej: "Persecucion").
public class PersecucionNiebla : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Opcional: si se deja vacío, busca a Dani en la escena.")]
    [SerializeField] private ControladorPersonaje jugador;
    [SerializeField] private FragmentoCorazon fragmento;
    [SerializeField] private NieblaPerseguidora niebla;
    [SerializeField] private InterfazMochila interfazMochila;

    [Header("Cámara")]
    [Tooltip("La Virtual Camera del riel. Dejala DESACTIVADA en la escena y con Priority mayor que la del FreeLook.")]
    [SerializeField] private GameObject camaraRiel;

    [Header("Recorrido de la niebla")]
    [Tooltip("Dónde aparece, unos metros detrás de la zona de inicio y en línea recta con la puerta.")]
    [SerializeField] private Transform puntoAparicionNiebla;
    [Tooltip("Dónde se traba: el marco de la puerta.")]
    [SerializeField] private Transform puntoDetencionNiebla;

    [Header("Eventos (opcional: sonido, música, ocultar la pared que tapa la cámara...)")]
    [SerializeField] private UnityEvent alIniciar;
    [SerializeField] private UnityEvent alEscapar;
    [SerializeField] private UnityEvent alReiniciar;

    private NivelDeAutoestima _autoestima;

    public EstadoPersecucion Estado { get; private set; } = EstadoPersecucion.Inactiva;

    private void Awake()
    {
        if (jugador == null) jugador = FindFirstObjectByType<ControladorPersonaje>();

        if (jugador != null) _autoestima = jugador.GetComponent<NivelDeAutoestima>();

        if (jugador == null || _autoestima == null || fragmento == null || niebla == null ||
            camaraRiel == null || puntoAparicionNiebla == null || puntoDetencionNiebla == null)
        {
            Debug.LogError("PersecucionNiebla: faltan referencias en el Inspector (o Dani no tiene NivelDeAutoestima).", this);
            enabled = false;
            return;
        }

        camaraRiel.SetActive(false);
        niebla.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_autoestima == null) return;

        _autoestima.OnConsumidaPorNiebla += AlSerDerrotada;
        _autoestima.OnReaparecida += AlReaparecer;
        niebla.OnAlcanzoAJugador += AlSerAlcanzada;
    }

    private void OnDisable()
    {
        if (_autoestima == null) return;

        _autoestima.OnConsumidaPorNiebla -= AlSerDerrotada;
        _autoestima.OnReaparecida -= AlReaparecer;
        niebla.OnAlcanzoAJugador -= AlSerAlcanzada;
    }

    // ---------------------------------------------------------------
    // Lo llaman las zonas (triggers)
    // ---------------------------------------------------------------
    public void IntentarIniciar()
    {
        if (Estado != EstadoPersecucion.Inactiva) return;

        // La condición es "Dani TIENE el fragmento", no "pasó por tal lado": si lo descarta o lo pierde, no arranca
        if (jugador.mochila == null || !jugador.mochila.Contiene(fragmento.Datos)) return;

        Estado = EstadoPersecucion.EnCurso;

        // Movimiento libre (adelante/atrás/izquierda/derecha) pero relativo a la cámara del riel.
        // Como esa cámara mira siempre en la misma dirección, "arriba" en el control es siempre
        // "hacia el fondo de la pantalla": es el movimiento de Little Nightmares.
        jugador.UsarCamaraFija(camaraRiel.transform);
        camaraRiel.SetActive(true);

        BloquearUso(true);

        niebla.gameObject.SetActive(true);
        niebla.Iniciar(puntoAparicionNiebla.position, puntoDetencionNiebla.position);

        alIniciar?.Invoke();
    }

    public void Terminar()
    {
        if (Estado != EstadoPersecucion.EnCurso) return;

        Estado = EstadoPersecucion.Completada;

        // La niebla NO se esconde ni se frena: sigue hasta el marco de la puerta y se queda ahí, pero ya no es peligrosa
        niebla.HacerInofensiva();

        camaraRiel.SetActive(false);
        jugador.UsarCamaraFreeLook();
        BloquearUso(false);

        alEscapar?.Invoke();
    }

    // ---------------------------------------------------------------
    // Reacciones
    // ---------------------------------------------------------------
    private void AlSerAlcanzada()
    {
        if (Estado != EstadoPersecucion.EnCurso)
        {
            Debug.LogWarning($"PersecucionNiebla: la niebla tocó a Dani pero el estado es {Estado}, no EnCurso. No se reinicia.", this);
            return;
        }

        _autoestima.Derrotar(); // dispara OnConsumidaPorNiebla -> AlSerDerrotada
    }

    // Se dispara apenas Dani cae (pozo o niebla): la niebla se congela mientras ella está derrotada
    private void AlSerDerrotada()
    {
        if (Estado != EstadoPersecucion.EnCurso) return;

        Estado = EstadoPersecucion.Fallida;
        niebla.Detener();
    }

    // Se dispara cuando Dani ya reapareció en el checkpoint
    private void AlReaparecer(Transform _)
    {
        // Si ya escapó, el fragmento es suyo y no se toca
        if (Estado == EstadoPersecucion.Completada) return;

        Estado = EstadoPersecucion.Inactiva;

        niebla.Detener();
        niebla.gameObject.SetActive(false);

        camaraRiel.SetActive(false);
        jugador.UsarCamaraFreeLook();
        BloquearUso(false);

        fragmento.VolverAlOrigen(jugador.mochila);

        alReiniciar?.Invoke();
    }

    private void BloquearUso(bool bloquear)
    {
        if (interfazMochila != null) interfazMochila.Bloquear(bloquear);
        if (jugador.gestionLinterna != null) jugador.gestionLinterna.UsoBloqueado = bloquear;
    }
}