using System.Collections;
using UnityEngine;
using UnityEngine.Events;
public class EsferaGuiaRuta : MonoBehaviour
{
    private enum Fase { Oculta, Esperando, Guiando, Terminada }

    [Header("Cuándo aparece (usá UNA de las dos formas)")]
    [Tooltip("Aparece sola cuando el GameProgressManager registra este ID (por ejemplo PuzzleAula). Vacío = no se usa.")]
    [SerializeField] private string idPuzzleParaActivar;
    [Tooltip("Si no hay ID, también se puede activar desde un UnityEvent llamando a EsferaGuiaRuta.Activar().")]
    [SerializeField] private bool activarAlInicio = false;

    [Header("Visual")]
    [Tooltip("El hijo con el modelo, luz y partículas de la esfera. Se muestra al activarse y se oculta al terminar.")]
    [SerializeField] private GameObject visual;
    [SerializeField] private float segundosAparecer = 0.6f;
    [SerializeField] private float segundosDesaparecer = 0.6f;

    [Header("Ruta")]
    [Tooltip("Los puntos en orden. El último es el destino. Ponelos a la altura a la que querés que flote la esfera.")]
    [SerializeField] private Transform[] ruta;
    [SerializeField] private float velocidad = 5f;
    [Tooltip("A qué distancia de un punto se considera que llegó y pasa al siguiente.")]
    [SerializeField] private float distanciaLlegada = 0.2f;
    [SerializeField] private bool desaparecerAlLlegar = true;

    [Header("Dani")]
    [Tooltip("Si queda vacío se busca solo.")]
    [SerializeField] private Transform jugador;
    [Tooltip("Distancia a la que tiene que acercarse Dani para que la esfera empiece a guiar.")]
    [SerializeField] private float distanciaActivacion = 4f;
    [Tooltip("Si Dani se aleja más que esto, la esfera se detiene y la espera.")]
    [SerializeField] private float distanciaEspera = 9f;
    [Tooltip("La esfera retoma la marcha cuando Dani vuelve a estar a menos de esto.")]
    [SerializeField] private float distanciaReanudar = 6f;

    [Header("Flotación")]
    [SerializeField] private float amplitud = 0.2f;
    [SerializeField] private float velocidadFlotacion = 2f;

    [Header("Mensaje de Dani al empezar a guiar (opcional)")]
    [SerializeField] private DialogoInicial dialogo;
    [SerializeField, TextArea] private string mensaje = "";
    [SerializeField] private float duracionMensaje = 3f;

    [Header("Eventos")]
    public UnityEvent alActivarse;
    public UnityEvent alEmpezarAGuiar;
    public UnityEvent alLlegar;

    private Fase _fase = Fase.Oculta;
    private Vector3 _base;            // posición "real" de la esfera, sin la flotación
    private int _indice;
    private bool _pausada;
    private Vector3 _escalaVisual = Vector3.one;
    private Coroutine _rutinaVisual;
    private GameProgressManager _progreso;

    private void Awake()
    {
        _base = transform.position;

        if (visual != null)
        {
            _escalaVisual = visual.transform.localScale;
            visual.SetActive(false);
        }

        if (jugador == null)
        {
            ControladorPersonaje dani = FindFirstObjectByType<ControladorPersonaje>();
            if (dani != null) jugador = dani.transform;
        }

        if (jugador == null)
            Debug.LogError("EsferaGuiaRuta: no encontré a Dani en la escena.", this);

        if (ruta == null || ruta.Length == 0)
            Debug.LogWarning("EsferaGuiaRuta: la ruta está vacía, la esfera no tiene a dónde ir.", this);
    }

    private void Start()
    {
        if (!string.IsNullOrWhiteSpace(idPuzzleParaActivar))
        {
            _progreso = GameProgressManager.Instancia;

            if (_progreso != null)
            {
                _progreso.AlCompletarPuzzle += AlCompletarPuzzle;

                // Por si el puzzle ya estaba completado
                if (_progreso.EstaCompletado(idPuzzleParaActivar)) Activar();
            }
            else
            {
                Debug.LogError("EsferaGuiaRuta: no encontré el GameProgressManager.", this);
            }
        }

        if (activarAlInicio) Activar();
    }

    private void OnDestroy()
    {
        if (_progreso != null) _progreso.AlCompletarPuzzle -= AlCompletarPuzzle;
    }

    private void AlCompletarPuzzle(string id)
    {
        if (id == idPuzzleParaActivar) Activar();
    }

    // Se puede llamar desde un UnityEvent (por ejemplo "Al Terminar" de un mensaje de fantasma)
    public void Activar()
    {
        if (_fase != Fase.Oculta) return;

        _fase = Fase.Esperando;
        _base = transform.position;

        if (visual != null)
        {
            visual.SetActive(true);
            Reiniciar(RutinaEscala(0f, 1f, segundosAparecer, false));
        }

        alActivarse?.Invoke();
    }

    private void Update()
    {
        if (_fase == Fase.Oculta) return;

        if (_fase == Fase.Esperando) ActualizarEspera();
        else if (_fase == Fase.Guiando) ActualizarGuia();

        // La flotación se aplica siempre que la esfera está visible
        transform.position = _base + Vector3.up * (Mathf.Sin(Time.time * velocidadFlotacion) * amplitud);
    }

    private void ActualizarEspera()
    {
        if (jugador == null || ruta == null || ruta.Length == 0) return;

        if (Distancia() <= distanciaActivacion)
        {
            _fase = Fase.Guiando;
            _indice = 0;
            _pausada = false;

            if (dialogo != null && !string.IsNullOrEmpty(mensaje))
                dialogo.MostrarDialogo(mensaje, duracionMensaje);

            alEmpezarAGuiar?.Invoke();
        }
    }

    private void ActualizarGuia()
    {
        // Esperar a Dani si se queda atrás (con histéresis para que no titubee)
        if (jugador != null)
        {
            float d = Distancia();
            if (!_pausada && d > distanciaEspera) _pausada = true;
            else if (_pausada && d < distanciaReanudar) _pausada = false;
        }

        if (_pausada) return;

        Transform objetivo = ruta[_indice];
        if (objetivo == null) { SiguientePunto(); return; }

        _base = Vector3.MoveTowards(_base, objetivo.position, velocidad * Time.deltaTime);

        if ((_base - objetivo.position).sqrMagnitude <= distanciaLlegada * distanciaLlegada)
            SiguientePunto();
    }

    private void SiguientePunto()
    {
        _indice++;
        if (_indice < ruta.Length) return;

        _fase = Fase.Terminada;
        alLlegar?.Invoke();

        if (desaparecerAlLlegar && visual != null)
            Reiniciar(RutinaEscala(1f, 0f, segundosDesaparecer, true));
    }

    private float Distancia()
    {
        return Vector3.Distance(jugador.position, _base);
    }

    private void Reiniciar(IEnumerator rutina)
    {
        if (_rutinaVisual != null) StopCoroutine(_rutinaVisual);
        _rutinaVisual = StartCoroutine(rutina);
    }

    // Agranda o achica el visual; si ocultarAlFinal, lo apaga cuando termina
    private IEnumerator RutinaEscala(float desde, float hasta, float duracion, bool ocultarAlFinal)
    {
        float t = 0f;
        Transform v = visual.transform;

        while (t < duracion)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duracion));
            v.localScale = _escalaVisual * Mathf.Lerp(desde, hasta, k);
            yield return null;
        }

        v.localScale = _escalaVisual * hasta;
        if (ocultarAlFinal) visual.SetActive(false);
        _rutinaVisual = null;
    }

    // Dibuja la ruta en la Scene para poder colocar los puntos con comodidad
    private void OnDrawGizmosSelected()
    {
        if (ruta == null || ruta.Length == 0) return;

        Gizmos.color = Color.cyan;
        Vector3 anterior = transform.position;

        for (int i = 0; i < ruta.Length; i++)
        {
            if (ruta[i] == null) continue;

            Gizmos.DrawLine(anterior, ruta[i].position);
            Gizmos.DrawWireSphere(ruta[i].position, i == ruta.Length - 1 ? 0.5f : 0.25f);
            anterior = ruta[i].position;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacion);
    }
}