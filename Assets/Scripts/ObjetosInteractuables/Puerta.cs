using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System.Collections;
using UnityEngine;

public class Puerta : MonoBehaviour, IInteractable
{
    public enum TipoApertura
    {
        Deslizar, // La puerta sube en línea recta (las puertas básicas de antes)
        Bisagra   // La puerta gira alrededor de su bisagra, como una puerta normal
    }

    [Header("Cerradura")]
    [Tooltip("Llave que abre esta puerta. Vacío = no necesita llave.")]
    [SerializeField] private DatosItem llaveRequerida;

    [Tooltip("Marcalo si la puerta NO se abre con E sino por un mecanismo (reflectores, palanca...). Se abre desde código con AbrirPorMecanismo().")]
    [SerializeField] private bool bloqueadaPorMecanismo;

    [Header("Tipo de apertura")]
    [Tooltip("Deslizar = sube en línea recta. Bisagra = gira como una puerta normal.")]
    [SerializeField] private TipoApertura tipoApertura = TipoApertura.Deslizar;

    [Header("Bisagra (solo si el tipo es Bisagra)")]
    [Tooltip("Objeto vacío ubicado EXACTAMENTE en la bisagra. Si lo dejás vacío se usa el pivote de esta puerta (el origen del modelo).")]
    [SerializeField] private Transform pivoteBisagra;

    [Tooltip("Eje de giro, en el espacio local del pivote. Normalmente Y (0,1,0). Si el modelo vino de Blender con otra orientación, probá Z (0,0,1). Seleccioná la puerta y mirá el gizmo amarillo.")]
    [SerializeField] private Vector3 ejeLocal = Vector3.up;

    [Tooltip("Cuántos grados se abre.")]
    [Range(0f, 170f)]
    [SerializeField] private float anguloApertura = 90f;

    [Tooltip("Grados por segundo.")]
    [SerializeField] private float velocidadGiro = 90f;

    [Tooltip("Si está marcado, la puerta se abre para el lado contrario a donde está Dani (no te golpea).")]
    [SerializeField] private bool abrirAlejandoseDelJugador = true;

    [Tooltip("Da vuelta el sentido del giro. Con 'Abrir Alejandose Del Jugador' invierte el resultado automático.")]
    [SerializeField] private bool invertirSentido;

    [Header("Deslizar (solo si el tipo es Deslizar)")]
    [SerializeField] private float alturaApertura = 5f;
    [SerializeField] private float velocidadApertura = 2f;

    [Header("UI")]
    [SerializeField] private GameObject cartelUI;

    private bool _abierta;

    private Vector3 _posicionCerrada;
    private Quaternion _rotacionCerrada;
    private Coroutine _animacionPuerta;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (_abierta) return;

        if (bloqueadaPorMecanismo)
        {
            Debug.Log("La puerta no cede: algo más la mantiene cerrada.");
            return;
        }

        if (llaveRequerida != null)
        {
            if (jugador.mochila == null || !jugador.mochila.TryUsar(llaveRequerida))
            {
                Debug.Log($"La puerta está cerrada. Necesitás: {llaveRequerida.nombre}");
                return;
            }
        }

        Abrir(jugador.transform);
    }

    // La llama el mecanismo que la controla (PuzzleReflectores)
    public void AbrirPorMecanismo()
    {
        if (_abierta) return;

        bloqueadaPorMecanismo = false;
        Abrir(null); // Sin jugador: usa el sentido por defecto
    }

    public void MostrarPista(bool mostrar)
    {
        // Si la abre un mecanismo, el cartel "E - Abrir" sería mentira
        if (cartelUI != null) cartelUI.SetActive(mostrar && !_abierta && !bloqueadaPorMecanismo);
    }

    public void CerrarPorEvento()
    {
        if (!_abierta || tipoApertura != TipoApertura.Bisagra)
            return;

        if (_animacionPuerta != null)
            StopCoroutine(_animacionPuerta);

        _animacionPuerta = StartCoroutine(RutinaCerrarBisagra());
    }

    private IEnumerator RutinaCerrarBisagra()
    {
        Vector3 posicionInicial = transform.position;
        Quaternion rotacionInicial = transform.rotation;

        float duracion = Quaternion.Angle(
            rotacionInicial,
            _rotacionCerrada
        ) / Mathf.Max(1f, velocidadGiro);

        duracion = Mathf.Max(0.01f, duracion);

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);

            transform.SetPositionAndRotation(
                Vector3.Lerp(
                    posicionInicial,
                    _posicionCerrada,
                    t
                ),
                Quaternion.Slerp(
                    rotacionInicial,
                    _rotacionCerrada,
                    t
                )
            );

            yield return null;
        }

        transform.SetPositionAndRotation(
            _posicionCerrada,
            _rotacionCerrada
        );

        _abierta = false;

        // Evitamos que Dani vuelva a abrirla durante el puzzle.
        bloqueadaPorMecanismo = true;

        _animacionPuerta = null;

        Debug.Log("La puerta del aula se cerró.");
    }

    private void Awake()
    {
        _posicionCerrada = transform.position;
        _rotacionCerrada = transform.rotation;
    }

    private void Abrir(Transform jugador)
    {
        _abierta = true;
        MostrarPista(false);

        if (_animacionPuerta != null)
            StopCoroutine(_animacionPuerta);

        if (tipoApertura == TipoApertura.Bisagra)
            _animacionPuerta = StartCoroutine(RutinaBisagra(jugador));
        else
            _animacionPuerta = StartCoroutine(RutinaDeslizar());
    }


    private IEnumerator RutinaBisagra(Transform jugador)
    {
        Transform pivote = pivoteBisagra != null ? pivoteBisagra : transform;

        // Todo se calcula UNA vez al empezar: así la puerta no acumula error de redondeo
        Vector3 puntoPivote = pivote.position;
        Vector3 eje = pivote.TransformDirection(ejeLocal).normalized;
        Vector3 posicionInicial = transform.position;
        Quaternion rotacionInicial = transform.rotation;

        float signo = 1f;
        if (abrirAlejandoseDelJugador && jugador != null)
            signo = SignoParaAlejarseDe(jugador.position, puntoPivote, eje);
        if (invertirSentido) signo = -signo;

        float objetivo = Mathf.Abs(anguloApertura);
        float velocidad = Mathf.Max(1f, velocidadGiro);
        float actual = 0f;

        while (actual < objetivo)
        {
            actual = Mathf.MoveTowards(actual, objetivo, velocidad * Time.deltaTime);

            Quaternion giro = Quaternion.AngleAxis(actual * signo, eje);
            transform.SetPositionAndRotation(
                puntoPivote + giro * (posicionInicial - puntoPivote),
                giro * rotacionInicial);

            yield return null;
        }
    }

    // +1 o -1: el sentido en que la hoja de la puerta se aleja del jugador
    private float SignoParaAlejarseDe(Vector3 posicionJugador, Vector3 puntoPivote, Vector3 eje)
    {
        Vector3 centroHoja = CentroDeLaHoja();
        Vector3 radio = Vector3.ProjectOnPlane(centroHoja - puntoPivote, eje);

        // Si el pivote está en el centro de la hoja no hay forma de saber hacia dónde barre
        if (radio.sqrMagnitude < 0.0001f) return 1f;

        // Hacia dónde se movería la hoja con un giro positivo
        Vector3 tangente = Quaternion.AngleAxis(10f, eje) * radio - radio;
        Vector3 haciaJugador = Vector3.ProjectOnPlane(posicionJugador - centroHoja, eje);

        // Si el giro positivo barre hacia el jugador, giramos al revés
        return Vector3.Dot(tangente, haciaJugador) > 0f ? -1f : 1f;
    }

    private Vector3 CentroDeLaHoja()
    {
        Renderer render = GetComponentInChildren<Renderer>();
        return render != null ? render.bounds.center : transform.position;
    }

    private IEnumerator RutinaDeslizar()
    {
        Vector3 destino = transform.position + Vector3.up * alturaApertura;

        while (transform.position != destino)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidadApertura * Time.deltaTime);
            yield return null;
        }
    }

    public void AbrirAlCompletarPuzzle()
    {
        StartCoroutine(RutinaAbrirAlCompletarPuzzle());
    }

    private IEnumerator RutinaAbrirAlCompletarPuzzle()
    {
        // Esperamos si todavía está terminando de cerrarse.
        while (_animacionPuerta != null)
        {
            yield return null;
        }

        if (_abierta)
            yield break;

        AbrirPorMecanismo();

        Debug.Log("Puerta abierta: puzzle del aula completado.");
    }

    // Ayuda visual en la Scene: esfera en la bisagra y línea a lo largo del eje de giro
    private void OnDrawGizmosSelected()
    {
        if (tipoApertura != TipoApertura.Bisagra) return;

        Transform pivote = pivoteBisagra != null ? pivoteBisagra : transform;
        Vector3 eje = pivote.TransformDirection(ejeLocal).normalized;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pivote.position, 0.15f);
        Gizmos.DrawLine(pivote.position - eje * 2f, pivote.position + eje * 2f);
    }
}