using System.Collections;
using TMPro;
using UnityEngine;

// Fantasmita bueno que hace de checkpoint. Es un niño que, como Dani, sufre bullying.
//
// - Cuando Dani se acerca, se "da cuenta" de ella y se vuelve su checkpoint.
// - Cuando Dani cae y reaparece acá, se le acerca preocupado, le dice algo de apoyo y vuelve a su lugar.
//
// ESTRUCTURA en la Hierarchy (importante):
//   FantasmaCheckpoint        <- este script. NO se mueve: es el ancla del checkpoint
//     ├─ Cuerpo               <- lo único que se mueve cuando va hacia Dani
//     │    ├─ Visual          <- el modelo (sin colliders). Flota suavemente
//     │    ├─ TextoMensaje    <- TextMeshPro 3D (burbuja de texto)
//     │    └─ IndicadorActivo <- opcional: luz/partículas
//     └─ PuntoReaparicion     <- donde aparece Dani. HERMANO de Cuerpo, no hijo
public class FantasmaCheckPoint : MonoBehaviour
{
    [Header("Checkpoint")]
    [Tooltip("Dónde reaparece Dani. Un Empty hijo de este objeto (NO de Cuerpo). Su flecha azul (Z) tiene que apuntar al fantasma: Dani reaparece mirándolo.")]
    [SerializeField] private Transform puntoReaparicion;
    [Tooltip("A qué distancia de Dani se activa como checkpoint.")]
    [SerializeField] private float radioActivacion = 3f;
    [SerializeField] private float intervaloChequeo = 0.2f;

    [Header("Apoyo a Dani")]
    [Tooltip("Hasta qué distancia de Dani se acerca para hablarle.")]
    [SerializeField] private float distanciaConversacion = 1.8f;
    [SerializeField] private float velocidadAcercamiento = 3f;
    [Tooltip("Si Dani se aleja y no la alcanza en este tiempo, habla igual.")]
    [SerializeField] private float tiempoMaxAcercamiento = 5f;
    [SerializeField] private float segundosMensaje = 3.5f;
    [SerializeField] private float segundosAparecer = 0.3f;
    [SerializeField] private float segundosDesvanecer = 0.5f;
    [TextArea]
    [SerializeField]
    private string[] mensajes =
    {
        "¡Dani, tú puedes! Sé fuerte.",
        "¿Estás bien? ¿Te lastimaron?",
        "Por favor, Dani, no te rindas.",
        "No temas."
    };

    [Header("Partes (Cuerpo es obligatorio, el resto opcional)")]
    [SerializeField] private Transform cuerpo;
    [SerializeField] private Transform visual;
    [SerializeField] private TMP_Text textoMensaje;
    [SerializeField] private GameObject indicadorActivo;

    [Header("Flote")]
    [SerializeField] private float amplitudFlote = 0.15f;
    [SerializeField] private float velocidadFlote = 1.5f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxActivar;
    [SerializeField] private AudioClip sfxMensaje;

    private const float VelocidadGiro = 8f;

    private Transform _jugador;
    private NivelDeAutoestima _autoestima;
    private Camera _camara;
    private Coroutine _rutinaApoyo;

    private Vector3 _cuerpoLocalInicial;
    private Quaternion _cuerpoRotacionInicial;
    private Vector3 _visualLocalInicial;

    private bool _esActivo;
    private float _timerChequeo;
    private int _ultimoIndice = -1;

    private void Awake()
    {
        ControladorPersonaje jugador = FindFirstObjectByType<ControladorPersonaje>();
        if (jugador != null)
        {
            _jugador = jugador.transform;
            _autoestima = jugador.GetComponent<NivelDeAutoestima>();
        }

        if (_jugador == null || _autoestima == null)
            Debug.LogError("FantasmaCheckpoint: no encontré a Dani con NivelDeAutoestima en la escena.", this);

        if (cuerpo == null)
        {
            Debug.LogWarning("FantasmaCheckpoint: falta asignar 'Cuerpo'. Uso el propio objeto: el punto de reaparición se movería con él.", this);
            cuerpo = transform;
        }

        if (puntoReaparicion == null)
        {
            Debug.LogWarning("FantasmaCheckpoint: falta 'Punto Reaparicion'. Dani reaparecería dentro del fantasma.", this);
            puntoReaparicion = transform;
        }

        _cuerpoLocalInicial = cuerpo.localPosition;
        _cuerpoRotacionInicial = cuerpo.localRotation;
        if (visual != null) _visualLocalInicial = visual.localPosition;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        OcultarMensaje();
        if (indicadorActivo != null) indicadorActivo.SetActive(false);
    }

    private void OnEnable()
    {
        if (_autoestima == null) return;

        _autoestima.OnCheckpointCambiado += AlCambiarCheckpoint;
        _autoestima.OnReaparecida += AlReaparecerDani;

        // Sincronizar con el checkpoint que ya esté activo (ej: el del inicio del nivel)
        AlCambiarCheckpoint(_autoestima.ultimoCheckpoint);
    }

    private void OnDisable()
    {
        if (_autoestima == null) return;

        _autoestima.OnCheckpointCambiado -= AlCambiarCheckpoint;
        _autoestima.OnReaparecida -= AlReaparecerDani;
    }

    private void Update()
    {
        // Flote suave del modelo
        if (visual != null)
            visual.localPosition = _visualLocalInicial + Vector3.up * (Mathf.Sin(Time.time * velocidadFlote) * amplitudFlote);

        if (_jugador == null || _autoestima == null || _esActivo) return;

        // Chequeo por distancia cada tanto: sin colliders ni tags, así no interfiere con raycasts ni capas
        _timerChequeo += Time.deltaTime;
        if (_timerChequeo < intervaloChequeo) return;
        _timerChequeo = 0f;

        if ((_jugador.position - transform.position).sqrMagnitude <= radioActivacion * radioActivacion)
            Activar();
    }

    // La burbuja de texto siempre mira a la cámara
    private void LateUpdate()
    {
        if (textoMensaje == null) return;

        if (_camara == null) _camara = Camera.main;
        if (_camara != null) textoMensaje.transform.rotation = _camara.transform.rotation;
    }

    // ---------------------------------------------------------------
    // Checkpoint
    // ---------------------------------------------------------------
    private void Activar()
    {
        _autoestima.SetCheckpoint(puntoReaparicion); // dispara AlCambiarCheckpoint
        Reproducir(sfxActivar);
    }

    private void AlCambiarCheckpoint(Transform nuevo)
    {
        _esActivo = nuevo == puntoReaparicion;
        if (indicadorActivo != null) indicadorActivo.SetActive(_esActivo);
    }

    // ---------------------------------------------------------------
    // Apoyo cuando Dani reaparece
    // ---------------------------------------------------------------
    private void AlReaparecerDani(Transform checkpoint)
    {
        if (checkpoint != puntoReaparicion || _jugador == null) return; // reapareció en otro lado

        if (_rutinaApoyo != null) StopCoroutine(_rutinaApoyo);
        OcultarMensaje();
        _rutinaApoyo = StartCoroutine(RutinaApoyo());
    }

    private IEnumerator RutinaApoyo()
    {
        // 1. Se le acerca preocupado
        float tiempo = 0f;
        while (tiempo < tiempoMaxAcercamiento &&
               DistanciaPlana(cuerpo.position, _jugador.position) > distanciaConversacion)
        {
            Vector3 destino = new Vector3(_jugador.position.x, cuerpo.position.y, _jugador.position.z);
            cuerpo.position = Vector3.MoveTowards(cuerpo.position, destino, velocidadAcercamiento * Time.deltaTime);
            MirarA(_jugador.position);

            tiempo += Time.deltaTime;
            yield return null;
        }

        // 2. Le dice el mensaje
        yield return StartCoroutine(RutinaMensaje(ElegirMensaje()));

        // 3. Vuelve a su lugar
        Vector3 casa = transform.TransformPoint(_cuerpoLocalInicial);
        while ((cuerpo.position - casa).sqrMagnitude > 0.0025f)
        {
            cuerpo.position = Vector3.MoveTowards(cuerpo.position, casa, velocidadAcercamiento * Time.deltaTime);
            MirarA(casa);
            yield return null;
        }

        cuerpo.localPosition = _cuerpoLocalInicial;
        cuerpo.localRotation = _cuerpoRotacionInicial;
        _rutinaApoyo = null;
    }

    private IEnumerator RutinaMensaje(string mensaje)
    {
        if (string.IsNullOrEmpty(mensaje)) yield break;

        Reproducir(sfxMensaje);

        if (textoMensaje == null)
        {
            // Sin burbuja asignada, al menos queda en la consola
            Debug.Log($"[Fantasma] {mensaje}");
            yield return new WaitForSeconds(segundosMensaje);
            yield break;
        }

        textoMensaje.text = mensaje;
        yield return StartCoroutine(Fundir(0f, 1f, segundosAparecer));

        // Se queda mirando a Dani mientras le habla
        float t = 0f;
        while (t < segundosMensaje)
        {
            MirarA(_jugador.position);
            t += Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(Fundir(1f, 0f, segundosDesvanecer));
        OcultarMensaje();
    }

    private IEnumerator Fundir(float desde, float hasta, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            textoMensaje.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        textoMensaje.alpha = hasta;
    }

    private void OcultarMensaje()
    {
        if (textoMensaje == null) return;

        textoMensaje.text = "";
        textoMensaje.alpha = 0f;
    }

    // Elige al azar, sin repetir el mismo dos veces seguidas
    private string ElegirMensaje()
    {
        if (mensajes == null || mensajes.Length == 0) return "";

        int indice = Random.Range(0, mensajes.Length);
        if (mensajes.Length > 1 && indice == _ultimoIndice)
            indice = (indice + Random.Range(1, mensajes.Length)) % mensajes.Length;

        _ultimoIndice = indice;
        return mensajes[indice];
    }

    // ---------------------------------------------------------------
    // Utilidades
    // ---------------------------------------------------------------
    private void MirarA(Vector3 punto)
    {
        Vector3 direccion = punto - cuerpo.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.0001f) return;

        cuerpo.rotation = Quaternion.Slerp(cuerpo.rotation, Quaternion.LookRotation(direccion), VelocidadGiro * Time.deltaTime);
    }

    private static float DistanciaPlana(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void Reproducir(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioActivacion);

        if (puntoReaparicion != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(puntoReaparicion.position, 0.25f);
            Gizmos.DrawRay(puntoReaparicion.position, puntoReaparicion.forward * 1.5f);
        }
    }
}