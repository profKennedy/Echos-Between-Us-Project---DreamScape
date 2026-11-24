using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Eventos")]
    [SerializeField] private UnityEvent alAbrirPuerta;

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

    [Header("Cierre de golpe (método CerrarDeGolpe)")]
    [Tooltip("Segundos que espera desde que se llama CerrarDeGolpe hasta que empieza a cerrarse.")]
    [SerializeField] private float retrasoCierreGolpe = 0.2f;

    [Tooltip("Bisagra: grados por segundo con los que se cierra de golpe. Acelera hasta pegar contra el marco.")]
    [SerializeField] private float velocidadCierreGolpe = 400f;

    [Tooltip("Deslizar: unidades por segundo con las que baja de golpe.")]
    [SerializeField] private float velocidadCierreDeslizarGolpe = 30f;

    [Tooltip("Se dispara cuando la puerta pega contra el marco: sonido de portazo, sacudida de cámara...")]
    [SerializeField] private UnityEvent alCerrarseDeGolpe;

    [Header("Colisión con Dani")]
    [Tooltip("Mientras la puerta se mueve (y hasta que Dani deje de tocarla), la puerta y Dani se atraviesan. Evita que Dani quede trabada dentro de la hoja al abrir o cerrar.")]
    [SerializeField] private bool ignorarColisionConJugadorAlMoverse = true;

    [Header("UI")]
    [SerializeField] private GameObject cartelUI;

    private bool _abierta;

    private Vector3 _posicionCerrada;
    private Quaternion _rotacionCerrada;
    private Coroutine _animacionPuerta;

    // Datos de la última apertura por bisagra: así se puede cerrar siguiendo el mismo arco
    private Vector3 _puntoPivoteApertura;
    private Vector3 _ejeApertura = Vector3.up;
    private float _signoApertura = 1f;
    private float _anguloAbierto;

    // Colisión puerta <-> Dani
    private Collider[] _collidersPuerta;
    private Collider[] _collidersJugador;
    private Coroutine _rutinaColisiones;

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
        IgnorarJugadorDuranteMovimiento();
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

        IgnorarJugadorDuranteMovimiento();

        alAbrirPuerta?.Invoke();
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

        _puntoPivoteApertura = puntoPivote;
        _ejeApertura = eje;
        _signoApertura = signo;
        _anguloAbierto = 0f;

        while (actual < objetivo)
        {
            actual = Mathf.MoveTowards(actual, objetivo, velocidad * Time.deltaTime);
            _anguloAbierto = actual;

            Quaternion giro = Quaternion.AngleAxis(actual * signo, eje);
            transform.SetPositionAndRotation(
                puntoPivote + giro * (posicionInicial - puntoPivote),
                giro * rotacionInicial);

            yield return null;
        }

        _animacionPuerta = null;
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
        _animacionPuerta = null;
    }

    // Cierra la puerta de golpe, como un portazo, y queda bloqueada para siempre.
    // Se puede llamar desde un UnityEvent (por ejemplo PersecucionNiebla > Al Escapar).
    public void CerrarDeGolpe()
    {
        if (!_abierta) return;

        if (_animacionPuerta != null)
            StopCoroutine(_animacionPuerta);

        _animacionPuerta = StartCoroutine(RutinaCierreDeGolpe());
        IgnorarJugadorDuranteMovimiento();
    }

    private IEnumerator RutinaCierreDeGolpe()
    {
        if (retrasoCierreGolpe > 0f)
            yield return new WaitForSeconds(retrasoCierreGolpe);

        if (tipoApertura == TipoApertura.Bisagra)
            yield return RutinaCerrarBisagraDeGolpe();
        else
            yield return RutinaCerrarDeslizarDeGolpe();

        transform.SetPositionAndRotation(_posicionCerrada, _rotacionCerrada);

        _abierta = false;
        bloqueadaPorMecanismo = true; // no se vuelve a abrir
        _animacionPuerta = null;

        alCerrarseDeGolpe?.Invoke();
    }

    private IEnumerator RutinaCerrarBisagraDeGolpe()
    {
        float anguloInicial = _anguloAbierto;
        float duracion = Mathf.Max(0.05f, anguloInicial / Mathf.Max(1f, velocidadCierreGolpe));
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);

            // t*t: arranca lento y acelera hasta pegar contra el marco
            float restante = anguloInicial * (1f - t * t);

            Quaternion giro = Quaternion.AngleAxis(restante * _signoApertura, _ejeApertura);
            transform.SetPositionAndRotation(
                _puntoPivoteApertura + giro * (_posicionCerrada - _puntoPivoteApertura),
                giro * _rotacionCerrada);

            yield return null;
        }
    }

    private IEnumerator RutinaCerrarDeslizarDeGolpe()
    {
        while (transform.position != _posicionCerrada)
        {
            transform.position = Vector3.MoveTowards(transform.position, _posicionCerrada, velocidadCierreDeslizarGolpe * Time.deltaTime);
            yield return null;
        }
    }

    // ---------------------------------------------------------------
    // Colisión puerta <-> Dani
    // La hoja se mueve por transform (no por física): si barre a Dani, el motor la empuja contra el
    // mesh y queda trabada. Mientras la puerta se mueve no se tocan, y solo vuelven a colisionar
    // cuando Dani ya no está pisando la puerta.
    // ---------------------------------------------------------------
    private void IgnorarJugadorDuranteMovimiento()
    {
        if (!ignorarColisionConJugadorAlMoverse) return;

        if (_collidersJugador == null || _collidersJugador.Length == 0)
        {
            ControladorPersonaje dani = FindFirstObjectByType<ControladorPersonaje>();
            if (dani == null) return;

            _collidersJugador = dani.GetComponentsInChildren<Collider>(true);
        }

        if (_collidersPuerta == null)
            _collidersPuerta = GetComponentsInChildren<Collider>(true);

        AlternarIgnorarJugador(true);

        if (_rutinaColisiones == null)
            _rutinaColisiones = StartCoroutine(RestaurarColisionesCuandoQuedeLibre());
    }

    private IEnumerator RestaurarColisionesCuandoQuedeLibre()
    {
        // 1. Esperar a que la puerta termine de moverse
        while (_animacionPuerta != null)
            yield return null;

        // 2. Esperar a que Dani no esté pisando la puerta
        while (HayContactoConJugador())
            yield return new WaitForSeconds(0.1f);

        AlternarIgnorarJugador(false);
        _rutinaColisiones = null;
    }

    private void AlternarIgnorarJugador(bool ignorar)
    {
        foreach (Collider puerta in _collidersPuerta)
        {
            if (puerta == null) continue;

            foreach (Collider jugador in _collidersJugador)
            {
                if (jugador == null || jugador == puerta) continue;
                Physics.IgnoreCollision(puerta, jugador, ignorar);
            }
        }
    }

    private bool HayContactoConJugador()
    {
        foreach (Collider puerta in _collidersPuerta)
        {
            if (puerta == null || !puerta.enabled || puerta.isTrigger) continue;

            foreach (Collider jugador in _collidersJugador)
            {
                if (jugador == null || !jugador.enabled || jugador.isTrigger) continue;
                if (SeSuperponen(puerta, jugador)) return true;
            }
        }

        return false;
    }

    private static bool SeSuperponen(Collider a, Collider b)
    {
        if (!a.bounds.Intersects(b.bounds)) return false;

        // Si alguno no soporta el cálculo exacto, nos quedamos con el chequeo de bounds (más conservador)
        if (!SoportaPenetracion(a) || !SoportaPenetracion(b)) return true;

        return Physics.ComputePenetration(
            a, a.transform.position, a.transform.rotation,
            b, b.transform.position, b.transform.rotation,
            out _, out _);
    }

    private static bool SoportaPenetracion(Collider c)
    {
        return c is BoxCollider || c is SphereCollider || c is CapsuleCollider
               || (c is MeshCollider malla && malla.convex);
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