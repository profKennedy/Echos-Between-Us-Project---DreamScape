using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Al entrar Dani a la biblioteca: la puerta se cierra y se bloquea, la autoestima baja al
// porcentaje configurado y Dani se encoge, se reproduce la cinemática de los libros y recién
// ahí recupera el control. Cuando recoge todos los libros, todo vuelve a la normalidad
// y la puerta se abre.
// Va en un GameObject con un BoxCollider en modo Trigger, ubicado justo adentro de la puerta.
[DefaultExecutionOrder(100)] // corre DESPUÉS del ReceptorMiradaFantasma (ver Update)
public class EncierroBiblioteca : MonoBehaviour
{
    [Header("Dani (si los dejás vacíos se buscan solos)")]
    [SerializeField] private NivelDeAutoestima autoestima;
    [SerializeField] private EfectoEncogimiento efectoEncogimiento;
    [SerializeField] private MovimientoPersonaje movimiento;
    [SerializeField] private GestorEntradas gestorEntradas;

    [Header("Puerta")]
    [SerializeField] private Puerta puerta;
    [SerializeField] private float retrasoCierre = 0.4f;

    [Header("Negatividad")]
    [Tooltip("Autoestima de Dani dentro de la biblioteca (0.5 = 50%). Es un TECHO: puede bajar más, pero no recuperarse por encima.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float porcentajeAutoestima = 0.5f;

    [Tooltip("Desmarcado: el tamaño sigue a la autoestima como siempre (50% = ~60% del tamaño). Marcado: se encoge al mínimo, como en el aula.")]
    [SerializeField] private bool encogerAlMinimo;

    [SerializeField] private float tiempoMaximoEncogimiento = 4f;
    [SerializeField] private float pausaTrasEncoger = 0.8f;

    [Header("Cinemática de los libros")]
    [SerializeField] private PresentacionLibros presentacion;

    [Header("Puzzle")]
    [SerializeField] private int librosNecesarios = 3;
    [SerializeField] private float retrasoApertura = 1f;
    [Tooltip("Opcional: ID que se registra en GameProgressManager al terminar.")]
    [SerializeField] private string idPuzzle = "";

    [Header("Eventos (ahí se pueden enganchar los fantasmas más adelante)")]
    [SerializeField] private UnityEvent alEncerrar;
    [Tooltip("Se dispara cuando termina la cinemática y Dani recupera el control. Ideal para que el fantasma guía empiece a hablar.")]
    [SerializeField] private UnityEvent alDevolverControl;
    [SerializeField] private UnityEvent alLiberar;

    private bool _iniciado;
    private bool _encerrada;
    private int _librosRecogidos;

    private void Awake()
    {
        if (autoestima == null || efectoEncogimiento == null || movimiento == null || gestorEntradas == null)
        {
            ControladorPersonaje dani = FindFirstObjectByType<ControladorPersonaje>();
            if (dani != null)
            {
                if (autoestima == null) autoestima = dani.GetComponent<NivelDeAutoestima>();
                if (efectoEncogimiento == null) efectoEncogimiento = dani.GetComponent<EfectoEncogimiento>();
                if (movimiento == null) movimiento = dani.movimiento;
                if (gestorEntradas == null) gestorEntradas = dani.entradas;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_iniciado || !other.CompareTag("Player")) return;

        if (autoestima == null || efectoEncogimiento == null)
        {
            Debug.LogError("EncierroBiblioteca: faltan NivelDeAutoestima o EfectoEncogimiento de Dani.", this);
            return;
        }

        StartCoroutine(SecuenciaEncierro());
    }

    // El ReceptorMiradaFantasma le devuelve 5 de autoestima por segundo a Dani cuando nadie la mira,
    // así que sin esto el 50% se recuperaría solo en 10 segundos. Acá lo usamos de techo.
    private void Update()
    {
        if (!_encerrada || autoestima == null) return;

        float techo = autoestima.autoestimaMaxima * porcentajeAutoestima;
        float exceso = autoestima.autoestimaActual - techo;
        if (exceso > 0.01f) autoestima.ReducirAutoestima(exceso);
    }

    private IEnumerator SecuenciaEncierro()
    {
        _iniciado = true;

        // 1. Dani quieta y sin entradas durante toda la secuencia
        BloquearDani(true);

        // 2. La puerta se cierra detrás y queda bloqueada
        yield return new WaitForSeconds(retrasoCierre);
        if (puerta != null) puerta.CerrarPorEvento();
        alEncerrar?.Invoke();

        // 3. Zona de alta negatividad: autoestima al techo y Dani se encoge
        _encerrada = true;
        if (encogerAlMinimo) efectoEncogimiento.EncogerPorPuzzle();

        float techo = autoestima.autoestimaMaxima * porcentajeAutoestima;
        float exceso = autoestima.autoestimaActual - techo;
        if (exceso > 0f) autoestima.ReducirAutoestima(exceso);

        float tiempo = 0f;
        while (Mathf.Abs(efectoEncogimiento.ObtenerFactorActual() - efectoEncogimiento.factorObjetivo) > 0.01f
               && tiempo < tiempoMaximoEncogimiento)
        {
            tiempo += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(pausaTrasEncoger);

        // 4. Plano de cada libro
        if (presentacion != null) yield return presentacion.Reproducir();

        // 5. Devolvemos el control
        BloquearDani(false);
        alDevolverControl?.Invoke();
    }

    // Lo llaman los LibroBiblioteca al ser recogidos
    public void RegistrarLibro()
    {
        _librosRecogidos++;
        Debug.Log($"Biblioteca: libro {_librosRecogidos}/{librosNecesarios}");

        if (_librosRecogidos >= librosNecesarios && _encerrada)
            StartCoroutine(SecuenciaLiberacion());
    }

    private IEnumerator SecuenciaLiberacion()
    {
        _encerrada = false; // deja de aplicarse el techo

        // Autoestima completa: el EfectoEncogimiento hace crecer a Dani solo
        if (encogerAlMinimo) efectoEncogimiento.RecuperarTamanoPorPuzzle();
        autoestima.RecuperarAutoestima(autoestima.autoestimaMaxima);

        if (!string.IsNullOrEmpty(idPuzzle) && GameProgressManager.Instancia != null)
            GameProgressManager.Instancia.RegistrarPuzzleCompletado(idPuzzle);

        yield return new WaitForSeconds(retrasoApertura);

        if (puerta != null) puerta.AbrirAlCompletarPuzzle();
        alLiberar?.Invoke();
    }

    private void BloquearDani(bool bloquear)
    {
        if (movimiento != null) movimiento.Bloquear(bloquear);
        if (gestorEntradas != null) gestorEntradas.enabled = !bloquear;
    }
}