using System;
using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Mensajes del fantasma bueno de la biblioteca, con el mismo formato que el recuerdo del gimnasio:
// Dani queda congelada, la cámara pasa a un plano fijo del fantasma y el texto aparece en pantalla.
//
//  - Mensaje INICIAL: dos formas de dispararlo desde el EncierroBiblioteca (evento "Al Devolver Control"):
//      * ReproducirInicio()    -> arranca enseguida.
//      * ArmarMensajeInicial() -> se "arma" y arranca recién cuando Dani se acerca al fantasma.
//  - Mensaje FINAL: se "arma" cuando Dani recoge todos los libros (evento "Al Liberar" del EncierroBiblioteca).
//    Cuando después se acerca al fantasma, se dispara UNA vez.
//
// Reemplaza al FantasmaGuia (la burbuja "Text_TextoMision"). Va en el mismo objeto del fantasma
// (Checkpoint_Biblioteca). El FantasmaCheckPoint sigue funcionando aparte.
public class MensajesFantasmaBiblioteca : MonoBehaviour
{
    [Serializable]
    public class Secuencia
    {
        [Tooltip("Cámara virtual fija que enfoca al fantasma. Tiene que estar activa y con Priority baja (5), como la del recuerdo del gimnasio.")]
        public CinemachineVirtualCamera camara;

        [TextArea(2, 4)]
        public string[] frases;

        [Tooltip("Se dispara cuando termina la secuencia y Dani ya recuperó el control.")]
        public UnityEvent alTerminar;
    }

    [Header("Mensaje inicial (después de la cinemática de los libros)")]
    [SerializeField]
    private Secuencia mensajeInicial = new Secuencia
    {
        frases = new[]
        {
            "Dani... este lugar te hace sentir pequeña.",
            "Para salir, tenés que encontrar los 3 libros.",
            "Acercate a cada uno y apretá E para recogerlo."
        }
    };

    [Header("Mensaje final (con los libros ya recogidos)")]
    [SerializeField]
    private Secuencia mensajeFinal = new Secuencia
    {
        frases = new[]
        {
            "Lo lograste, Dani. Ya no sos tan pequeña como este lugar te hacía sentir.",
            "El camino al teatro está abierto. Ahí está lo último que tenés que enfrentar.",
            "No estás sola. Yo voy a estar acá."
        }
    };

    [Header("Activación por cercanía (si se usan ArmarMensajeInicial / ArmarMensajeFinal)")]
    [SerializeField] private float radioActivacion = 4f;
    [SerializeField] private float intervaloChequeo = 0.2f;

    [Header("Fantasma")]
    [Tooltip("Lo que gira para mirar a Dani mientras habla (el 'Cuerpo' del fantasma). Opcional.")]
    [SerializeField] private Transform fantasma;

    [Header("Interfaz (podés usar el mismo panel del recuerdo del gimnasio)")]
    [SerializeField] private GameObject panelMensaje;
    [SerializeField] private TMP_Text textoMensaje;

    [Header("Dani (los vacíos se buscan solos)")]
    [SerializeField] private ControladorPersonaje jugador;
    [Tooltip("La FreeLook de Dani: se le frenan los ejes mientras dura el mensaje.")]
    [SerializeField] private CinemachineFreeLook camaraDani;
    [SerializeField] private InterfazMochila interfazMochila;
    [Tooltip("Los controles táctiles: se ocultan mientras dura el mensaje.")]
    [SerializeField] private GameObject panelControlesMobile;

    [Header("Cámara")]
    [Tooltip("Tiene que ser mayor que la Priority de la cámara de Dani (que puede haber quedado en 30 después del aula).")]
    [SerializeField] private int prioridadCamara = 70;
    [Tooltip("Solo se usa si no hay CinemachineBrain en la escena.")]
    [SerializeField] private float esperaSinBrain = 1f;

    [Header("Ritmo del texto")]
    [SerializeField] private float esperaAntesDeHablar = 0.5f;
    [Tooltip("Letras por segundo. 0 = aparece todo el texto de golpe.")]
    [SerializeField] private float letrasPorSegundo = 30f;
    [SerializeField] private float lecturaMinima = 2f;
    [Tooltip("Segundos extra de lectura por cada letra de la frase.")]
    [SerializeField] private float segundosPorLetra = 0.05f;
    [SerializeField] private float pausaEntreFrases = 0.4f;
    [SerializeField] private float esperaAlTerminar = 0.5f;

    private const float VelocidadGiro = 8f;

    private GestorEntradas _gestorEntradas;
    private CinemachineBrain _brain;

    private bool _ocupado;
    private bool _inicioArmado;
    private bool _inicioHecho;
    private bool _finalArmado;
    private bool _finalHecho;
    private float _timerChequeo;

    // Estado de Dani para restaurarlo igual que estaba
    private bool _entradasEstabanActivas;
    private bool _mobileEstabaActivo;
    private float _velocidadXAnterior;
    private float _velocidadYAnterior;
    private bool _jugadorBloqueado;

    // Para poder dejar todo en orden si el objeto se desactiva a la mitad
    private CinemachineVirtualCamera _camaraActiva;
    private int _prioridadOriginal;

    private void Awake()
    {
        if (jugador == null) jugador = FindFirstObjectByType<ControladorPersonaje>();

        if (jugador != null) _gestorEntradas = jugador.entradas;
        else Debug.LogError("MensajesFantasmaBiblioteca: no encontré a Dani en la escena.", this);

        _brain = FindFirstObjectByType<CinemachineBrain>();

        if (panelMensaje != null) panelMensaje.SetActive(false);
    }

    private void OnDisable()
    {
        // Si algo apaga este objeto en medio de un mensaje, Dani no puede quedar congelada
        if (!_ocupado) return;

        _ocupado = false;
        if (panelMensaje != null) panelMensaje.SetActive(false);
        if (_camaraActiva != null) _camaraActiva.Priority = _prioridadOriginal;
        BloquearJugador(false);
    }

    private void Update()
    {
        if (_ocupado || jugador == null) return;

        bool esperaInicio = _inicioArmado && !_inicioHecho;
        bool esperaFinal = _finalArmado && !_finalHecho;
        if (!esperaInicio && !esperaFinal) return;

        _timerChequeo += Time.deltaTime;
        if (_timerChequeo < intervaloChequeo) return;
        _timerChequeo = 0f;

        if ((jugador.transform.position - transform.position).sqrMagnitude > radioActivacion * radioActivacion) return;

        if (esperaInicio) ReproducirInicio();
        else ReproducirFinal();
    }

    // ---------------------------------------------------------------
    // API pública (para los UnityEvents del EncierroBiblioteca)
    // ---------------------------------------------------------------

    // Enganchar en EncierroBiblioteca > "Al Devolver Control"
    public void ReproducirInicio()
    {
        if (_inicioHecho || _ocupado) return;

        _inicioHecho = true;
        StartCoroutine(Reproducir(mensajeInicial));
    }

    // Alternativa a ReproducirInicio: el mensaje empieza cuando Dani se acerca al fantasma
    public void ArmarMensajeInicial()
    {
        _inicioArmado = true;
    }

    // Enganchar en EncierroBiblioteca > "Al Liberar"
    public void ArmarMensajeFinal()
    {
        _finalArmado = true;
    }

    private void ReproducirFinal()
    {
        if (_finalHecho || _ocupado) return;

        _finalHecho = true;
        StartCoroutine(Reproducir(mensajeFinal));
    }

    // ---------------------------------------------------------------
    // Secuencia
    // ---------------------------------------------------------------
    private IEnumerator Reproducir(Secuencia secuencia)
    {
        _ocupado = true;
        BloquearJugador(true);

        // 1. Cámara fija sobre el fantasma (esperamos a que termine el blend)
        _camaraActiva = secuencia.camara;
        if (_camaraActiva != null)
        {
            _prioridadOriginal = _camaraActiva.Priority;
            _camaraActiva.gameObject.SetActive(true);
            _camaraActiva.Priority = prioridadCamara;
            yield return EsperarBlend();
        }

        yield return Esperar(esperaAntesDeHablar);

        // 2. Las frases, una por una
        if (panelMensaje != null) panelMensaje.SetActive(true);

        if (secuencia.frases != null)
        {
            foreach (string frase in secuencia.frases)
            {
                if (string.IsNullOrEmpty(frase)) continue;

                yield return DecirFrase(frase);
                yield return Esperar(pausaEntreFrases);
            }
        }

        if (textoMensaje != null) textoMensaje.text = "";
        if (panelMensaje != null) panelMensaje.SetActive(false);

        yield return Esperar(esperaAlTerminar);

        // 3. Volvemos a la cámara de Dani y recién ahí le devolvemos el control
        if (_camaraActiva != null)
        {
            _camaraActiva.Priority = _prioridadOriginal;
            yield return EsperarBlend();
            _camaraActiva = null;
        }

        BloquearJugador(false);
        _ocupado = false;

        secuencia.alTerminar?.Invoke();
    }

    private IEnumerator DecirFrase(string frase)
    {
        if (textoMensaje == null)
        {
            // Sin texto asignado, al menos queda en la consola
            Debug.Log($"[Fantasma] {frase}");
            yield return Esperar(lecturaMinima);
            yield break;
        }

        textoMensaje.text = frase;
        textoMensaje.maxVisibleCharacters = letrasPorSegundo > 0f ? 0 : int.MaxValue;

        if (letrasPorSegundo > 0f)
        {
            textoMensaje.ForceMeshUpdate();
            int total = textoMensaje.textInfo.characterCount;
            float visibles = 0f;

            while (visibles < total)
            {
                visibles += letrasPorSegundo * Time.deltaTime;
                textoMensaje.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(visibles));
                MirarAJugador();
                yield return null;
            }

            textoMensaje.maxVisibleCharacters = int.MaxValue;
        }

        // Tiempo para leer, proporcional al largo de la frase
        yield return Esperar(Mathf.Max(lecturaMinima, frase.Length * segundosPorLetra));
    }

    private IEnumerator Esperar(float segundos)
    {
        float t = 0f;
        while (t < segundos)
        {
            MirarAJugador();
            t += Time.deltaTime;
            yield return null;
        }
    }

    // Espera a que Cinemachine termine de mezclar entre cámaras (mismo criterio que PresentacionLibros)
    private IEnumerator EsperarBlend()
    {
        if (_brain == null)
        {
            yield return new WaitForSeconds(esperaSinBrain);
            yield break;
        }

        // El Brain registra el cambio de cámara en su LateUpdate: hay que dejar pasar un frame
        yield return null;

        float limite = 6f; // por si algo se traba, nunca esperamos más que esto
        while (_brain.IsBlending && limite > 0f)
        {
            limite -= Time.deltaTime;
            yield return null;
        }
    }

    // ---------------------------------------------------------------
    // Dani congelada (mismo criterio que PresentacionFantasma / EncierroBiblioteca)
    // ---------------------------------------------------------------
    private void BloquearJugador(bool bloquear)
    {
        if (bloquear == _jugadorBloqueado) return;
        _jugadorBloqueado = bloquear;

        if (bloquear)
        {
            if (jugador != null) jugador.movimiento.Bloquear(true);
            if (interfazMochila != null) interfazMochila.Bloquear(true);

            if (panelControlesMobile != null)
            {
                _mobileEstabaActivo = panelControlesMobile.activeSelf;
                panelControlesMobile.SetActive(false);
            }

            if (_gestorEntradas != null)
            {
                _gestorEntradas.entradaMovimiento = Vector2.zero;
                _gestorEntradas.EstablecerMovimientoMobile(Vector2.zero);

                _entradasEstabanActivas = _gestorEntradas.enabled;
                _gestorEntradas.enabled = false;
            }

            if (camaraDani != null)
            {
                _velocidadXAnterior = camaraDani.m_XAxis.m_MaxSpeed;
                _velocidadYAnterior = camaraDani.m_YAxis.m_MaxSpeed;

                camaraDani.m_XAxis.m_MaxSpeed = 0f;
                camaraDani.m_YAxis.m_MaxSpeed = 0f;
            }
        }
        else
        {
            if (camaraDani != null)
            {
                camaraDani.m_XAxis.m_MaxSpeed = _velocidadXAnterior;
                camaraDani.m_YAxis.m_MaxSpeed = _velocidadYAnterior;
            }

            if (_gestorEntradas != null) _gestorEntradas.enabled = _entradasEstabanActivas;
            if (panelControlesMobile != null) panelControlesMobile.SetActive(_mobileEstabaActivo);

            if (interfazMochila != null) interfazMochila.Bloquear(false);
            if (jugador != null) jugador.movimiento.Bloquear(false);
        }
    }

    private void MirarAJugador()
    {
        if (fantasma == null || jugador == null) return;

        Vector3 direccion = jugador.transform.position - fantasma.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.0001f) return;

        fantasma.rotation = Quaternion.Slerp(fantasma.rotation, Quaternion.LookRotation(direccion), VelocidadGiro * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioActivacion);
    }
}