using System.Collections;
using TMPro;
using UnityEngine;

// Fantasmita bueno que le da instrucciones a Dani con la misma burbuja de texto (TMP) del FantasmaCheckPoint.
// No se mueve ni bloquea a Dani: se queda mirándola mientras le habla.
//
// Cuándo habla:
//  - Cuando algo llama a Comenzar() (por ejemplo el evento "Al Devolver Control" del EncierroBiblioteca), o
//  - Cuando Dani se acerca, si "Comenzar Al Acercarse" está marcado.
// Si Dani se vuelve a acercar más tarde, repite el recordatorio (si hay uno escrito).
//
// ESTRUCTURA (igual que el FantasmaCheckPoint):
//   FantasmaGuia          <- este script
//     ├─ Cuerpo           <- lo que mira a Dani (opcional, si falta se usa el propio objeto)
//     │    ├─ Visual      <- el modelo. Flota suavemente (opcional)
//     │    └─ TextoMensaje<- TextMeshPro 3D (la burbuja)
public class FantasmaGuia : MonoBehaviour
{
    [Header("Instrucciones (se dicen en orden, una por una)")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] instrucciones =
    {
        "Dani... este lugar te hace sentir pequeña.",
        "Para salir, tenés que encontrar los 3 libros.",
        "Acercate a cada uno y apretá E para recogerlo."
    };

    [Tooltip("Lo que repite si Dani se vuelve a acercar después de las instrucciones. Vacío = no repite nada.")]
    [TextArea(2, 4)]
    [SerializeField] private string recordatorio = "Encontrá los 3 libros, Dani. Vos podés.";

    [Header("Cuándo habla")]
    [SerializeField] private bool comenzarAlAcercarse;
    [SerializeField] private float radioActivacion = 4f;
    [SerializeField] private float intervaloChequeo = 0.2f;
    [Tooltip("Segundos mínimos entre un recordatorio y el siguiente.")]
    [SerializeField] private float esperaEntreRecordatorios = 15f;

    [Header("Ritmo del diálogo")]
    [Tooltip("Letras por segundo. 0 = aparece todo el texto de golpe.")]
    [SerializeField] private float letrasPorSegundo = 35f;
    [SerializeField] private float lecturaMinima = 1.5f;
    [Tooltip("Segundos extra de lectura por cada letra del mensaje.")]
    [SerializeField] private float segundosPorLetra = 0.04f;
    [SerializeField] private float pausaEntreMensajes = 0.4f;
    [SerializeField] private float segundosAparecer = 0.3f;
    [SerializeField] private float segundosDesvanecer = 0.4f;

    [Header("Partes (todas opcionales salvo el texto)")]
    [SerializeField] private Transform cuerpo;
    [SerializeField] private Transform visual;
    [SerializeField] private TMP_Text textoMensaje;

    [Header("Flote")]
    [SerializeField] private float amplitudFlote = 0.15f;
    [SerializeField] private float velocidadFlote = 1.5f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxMensaje;

    private const float VelocidadGiro = 8f;

    private Transform _jugador;
    private Camera _camara;
    private bool _hablando;
    private Vector3 _visualLocalInicial;
    private float _timerChequeo;
    private float _tiempoUltimoRecordatorio = float.NegativeInfinity;
    private bool _instruccionesDichas;

    private void Awake()
    {
        ControladorPersonaje jugador = FindFirstObjectByType<ControladorPersonaje>();
        if (jugador != null) _jugador = jugador.transform;
        else Debug.LogError("FantasmaGuia: no encontré a Dani en la escena.", this);

        if (cuerpo == null) cuerpo = transform;
        if (visual != null) _visualLocalInicial = visual.localPosition;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        OcultarMensaje();
    }

    private void Update()
    {
        if (visual != null)
            visual.localPosition = _visualLocalInicial + Vector3.up * (Mathf.Sin(Time.time * velocidadFlote) * amplitudFlote);

        if (_jugador == null || _hablando) return;

        _timerChequeo += Time.deltaTime;
        if (_timerChequeo < intervaloChequeo) return;
        _timerChequeo = 0f;

        if ((_jugador.position - transform.position).sqrMagnitude > radioActivacion * radioActivacion) return;

        if (!_instruccionesDichas)
        {
            if (comenzarAlAcercarse) Comenzar();
        }
        else if (!string.IsNullOrEmpty(recordatorio) &&
                 Time.time - _tiempoUltimoRecordatorio >= esperaEntreRecordatorios)
        {
            StartCoroutine(RutinaDialogo(new[] { recordatorio }));
        }
    }

    // La burbuja de texto siempre mira a la cámara
    private void LateUpdate()
    {
        if (textoMensaje == null) return;

        if (_camara == null) _camara = Camera.main;
        if (_camara != null) textoMensaje.transform.rotation = _camara.transform.rotation;
    }

    // Se puede llamar desde un UnityEvent (por ejemplo "Al Devolver Control" del EncierroBiblioteca)
    public void Comenzar()
    {
        if (_instruccionesDichas || _hablando) return;
        if (instrucciones == null || instrucciones.Length == 0) return;

        StartCoroutine(RutinaDialogo(instrucciones));
    }

    private IEnumerator RutinaDialogo(string[] mensajes)
    {
        _hablando = true;

        foreach (string mensaje in mensajes)
        {
            if (string.IsNullOrEmpty(mensaje)) continue;

            yield return StartCoroutine(DecirMensaje(mensaje));
            yield return new WaitForSeconds(pausaEntreMensajes);
        }

        _instruccionesDichas = true;
        _tiempoUltimoRecordatorio = Time.time;
        _hablando = false;
    }

    private IEnumerator DecirMensaje(string mensaje)
    {
        if (audioSource != null && sfxMensaje != null) audioSource.PlayOneShot(sfxMensaje);

        if (textoMensaje == null)
        {
            // Sin burbuja asignada, al menos queda en la consola
            Debug.Log($"[FantasmaGuia] {mensaje}");
            yield return new WaitForSeconds(lecturaMinima);
            yield break;
        }

        textoMensaje.text = mensaje;
        textoMensaje.maxVisibleCharacters = letrasPorSegundo > 0f ? 0 : int.MaxValue;

        // Con efecto de escritura el texto va apareciendo letra a letra; sin él, hace fade-in
        if (letrasPorSegundo > 0f)
        {
            textoMensaje.alpha = 1f;
            textoMensaje.ForceMeshUpdate();
            int total = textoMensaje.textInfo.characterCount;
            float visibles = 0f;
            while (visibles < total)
            {
                visibles += letrasPorSegundo * Time.deltaTime;
                textoMensaje.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(visibles));
                MirarA(_jugador);
                yield return null;
            }
            textoMensaje.maxVisibleCharacters = int.MaxValue;
        }
        else
        {
            yield return StartCoroutine(Fundir(0f, 1f, segundosAparecer));
        }

        // Tiempo para leer, proporcional al largo del mensaje
        float lectura = Mathf.Max(lecturaMinima, mensaje.Length * segundosPorLetra);
        float t = 0f;
        while (t < lectura)
        {
            MirarA(_jugador);
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
        textoMensaje.maxVisibleCharacters = int.MaxValue;
    }

    private void MirarA(Transform objetivo)
    {
        if (objetivo == null) return;

        Vector3 direccion = objetivo.position - cuerpo.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.0001f) return;

        cuerpo.rotation = Quaternion.Slerp(cuerpo.rotation, Quaternion.LookRotation(direccion), VelocidadGiro * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioActivacion);
    }
}