using System.Collections;
using Cinemachine;
using UnityEngine;

// "Director" del plano del escenario del teatro. Cuando un InterruptorReflector baja su palanca, le pide
// a este script que encienda el reflector CON plano:
//   1. Dani queda congelada (como en el gimnasio y la biblioteca).
//   2. La cámara del escenario toma el control y espera a que Cinemachine termine de mezclar.
//   3. La cámara queda QUIETA un momento mirando el escenario oscuro.
//   4. Con la cámara quieta, el reflector parpadea y se prende (el parpadeo lo hace el ReflectorEscenario).
//   5. Recién con la luz fija, la cámara hace un movimiento leve (acercamiento + deriva + giro) para apreciarla.
//   6. Vuelve a la cámara de Dani y recién ahí Dani recupera el control.
// Va en un GameObject vacío cualquiera (por ejemplo "PlanoEscenarioReflectores"). Hay UNO solo para los 4 interruptores.
public class PlanoEscenarioReflectores : MonoBehaviour
{
    [Header("Cámara del escenario")]
    [Tooltip("La cámara virtual que apunta al escenario. Tiene que estar activa y con Priority baja (5).")]
    [SerializeField] private CinemachineVirtualCamera camaraEscenario;
    [Tooltip("Tiene que ser mayor que la Priority de la cámara de Dani (puede haber quedado en 30 después del aula).")]
    [SerializeField] private int prioridadCamara = 70;
    [Tooltip("Solo se usa si no hay CinemachineBrain en la escena.")]
    [SerializeField] private float esperaSinBrain = 1f;

    [Header("Tiempos del plano")]
    [Tooltip("Segundos que la cámara se queda QUIETA en el escenario antes de que empiece a parpadear la luz.")]
    [SerializeField] private float retrasoEncendido = 1f;
    [Tooltip("Segundos que dura el movimiento leve de la cámara, que empieza cuando la luz ya quedó fija.")]
    [SerializeField] private float duracionMovimiento = 3f;
    [Tooltip("Pausa final antes de volver a la cámara de Dani.")]
    [SerializeField] private float pausaFinal = 0.3f;

    [Header("Movimiento leve de la cámara")]
    [Tooltip("Metros que avanza hacia adelante durante el movimiento.")]
    [SerializeField] private float empuje = 0.6f;
    [Tooltip("Metros que se corre hacia un costado. Alterna izquierda/derecha entre reflectores.")]
    [SerializeField] private float deriva = 0.25f;
    [Tooltip("Grados que gira en horizontal durante el movimiento (0 = no gira). Si la cámara tiene Look At, el giro lo decide Cinemachine.")]
    [SerializeField] private float giroGrados = 3f;

    [Header("Dani (los vacíos se buscan solos)")]
    [SerializeField] private ControladorPersonaje jugador;
    [Tooltip("La FreeLook de Dani: se le frenan los ejes mientras dura el plano.")]
    [SerializeField] private CinemachineFreeLook camaraDani;
    [SerializeField] private InterfazMochila interfazMochila;
    [Tooltip("Los controles táctiles: se ocultan mientras dura el plano.")]
    [SerializeField] private GameObject panelControlesMobile;

    private GestorEntradas _gestorEntradas;
    private CinemachineBrain _brain;

    private bool _ocupado;
    private int _planosHechos;

    // Estado de Dani para restaurarlo igual que estaba
    private bool _jugadorBloqueado;
    private bool _entradasEstabanActivas;
    private bool _mobileEstabaActivo;
    private float _velocidadXAnterior;
    private float _velocidadYAnterior;

    // Para dejar todo en orden si el objeto se desactiva a la mitad
    private int _prioridadOriginal;
    private Vector3 _posicionOriginal;
    private Quaternion _rotacionOriginal;

    private void Awake()
    {
        if (jugador == null) jugador = FindFirstObjectByType<ControladorPersonaje>();

        if (jugador != null) _gestorEntradas = jugador.entradas;
        else Debug.LogError("PlanoEscenarioReflectores: no encontré a Dani en la escena.", this);

        _brain = FindFirstObjectByType<CinemachineBrain>();

        if (camaraEscenario == null)
            Debug.LogWarning("PlanoEscenarioReflectores: falta la cámara del escenario. Los reflectores se encienden sin plano.", this);
    }

    private void OnDisable()
    {
        if (!_ocupado) return;

        _ocupado = false;
        RestaurarCamara();
        BloquearJugador(false);
    }

    // Lo llama el InterruptorReflector. Si por algún motivo ya hay un plano en curso, enciende igual sin plano
    // (el reflector nunca se pierde).
    public void EncenderConPlano(ReflectorEscenario reflector)
    {
        if (reflector == null) return;

        if (_ocupado || camaraEscenario == null)
        {
            reflector.Encender();
            return;
        }

        StartCoroutine(RutinaPlano(reflector));
    }

    private IEnumerator RutinaPlano(ReflectorEscenario reflector)
    {
        _ocupado = true;
        BloquearJugador(true);

        Transform t = camaraEscenario.transform;
        _posicionOriginal = t.position;
        _rotacionOriginal = t.rotation;
        _prioridadOriginal = camaraEscenario.Priority;

        float sentido = (_planosHechos % 2 == 0) ? 1f : -1f;
        _planosHechos++;

        Vector3 destino = _posicionOriginal + t.forward * empuje + t.right * (deriva * sentido);
        Quaternion giroFinal = Quaternion.AngleAxis(giroGrados * sentido, Vector3.up) * _rotacionOriginal;

        // 1. Cámara al escenario; el tiempo del plano empieza cuando el blend terminó
        camaraEscenario.gameObject.SetActive(true);
        camaraEscenario.Priority = prioridadCamara;
        yield return EsperarBlend();

        // 2. Cámara quieta: se ve el escenario oscuro antes de que pase nada
        yield return new WaitForSeconds(retrasoEncendido);

        // 3. La luz parpadea y se prende con la cámara quieta. Esperamos a que quede fija
        //    (margen de 10 s por si algo se traba: nunca dejamos a Dani congelada para siempre)
        reflector.Encender();

        float limite = 10f;
        while (!reflector.EstaEncendido && limite > 0f)
        {
            limite -= Time.deltaTime;
            yield return null;
        }

        // 4. Recién ahora la cámara se mueve, para apreciar la luz ya encendida
        float duracion = Mathf.Max(0.1f, duracionMovimiento);
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / duracion));
            t.position = Vector3.Lerp(_posicionOriginal, destino, k);
            t.rotation = Quaternion.Slerp(_rotacionOriginal, giroFinal, k);

            yield return null;
        }

        yield return new WaitForSeconds(pausaFinal);

        // 5. Volvemos a Dani y recién ahí le devolvemos el control
        camaraEscenario.Priority = _prioridadOriginal;
        yield return EsperarBlend();

        RestaurarCamara();
        BloquearJugador(false);
        _ocupado = false;
    }

    // Deja la cámara del escenario como estaba (posición, rotación y prioridad)
    private void RestaurarCamara()
    {
        if (camaraEscenario == null) return;

        camaraEscenario.Priority = _prioridadOriginal;
        camaraEscenario.transform.SetPositionAndRotation(_posicionOriginal, _rotacionOriginal);
    }

    // Espera a que Cinemachine termine de mezclar entre cámaras. Además de mirar IsBlending, espera como mínimo
    // el tiempo del blend por defecto del Brain: así no depende de cuándo el Brain registra el cambio de cámara.
    private IEnumerator EsperarBlend()
    {
        if (_brain == null)
        {
            yield return new WaitForSeconds(esperaSinBrain);
            yield break;
        }

        float minimo = _brain.m_DefaultBlend.BlendTime;
        float transcurrido = 0f;
        float limite = 6f; // por si algo se traba, nunca esperamos más que esto

        // Un frame para que el Brain registre el cambio de cámara en su LateUpdate
        yield return null;

        while ((_brain.IsBlending || transcurrido < minimo) && limite > 0f)
        {
            float dt = Time.deltaTime;
            transcurrido += dt;
            limite -= dt;
            yield return null;
        }
    }

    // Dani congelada (mismo criterio que MensajesFantasmaBiblioteca / PresentacionFantasma)
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
}