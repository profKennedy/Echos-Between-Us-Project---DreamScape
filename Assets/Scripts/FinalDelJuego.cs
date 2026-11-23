using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Cierre del juego: la "vuelta a la realidad".
//
// Se llama desde el evento "Al Terminar" del mensaje de la maestra (MensajesFantasmaBiblioteca):
//   1. Congela a Dani otra vez (el mensaje de la maestra le devuelve el control al terminar).
//   2. Suena el timbre (opcional) y la pantalla se cubre de a poco, bajando también los audios del ambiente.
//   3. Se muestran las líneas de texto, una por una, con fade.
//   4. Se carga la escena de créditos. Si la escena no está en Build Settings, vuelve al menú
//      en lugar de dejar el juego trabado.
//
// ESTRUCTURA en la Hierarchy:
//   CanvasFinal (Canvas Overlay, Sort Order alto)
//     ├─ PantallaRealidad  (Image, estirada a toda la pantalla)
//     └─ TextoRealidad     (TextMeshPro, ENCIMA de la imagen)
//   FinalDelJuego          (vacío con este script)
public class FinalDelJuego : MonoBehaviour
{
    [Header("Dani (los vacíos se buscan solos, salvo la mochila y los controles mobile)")]
    [SerializeField] private ControladorPersonaje jugador;
    [SerializeField] private InterfazMochila interfazMochila;
    [Tooltip("Los controles táctiles: se ocultan durante el final.")]
    [SerializeField] private GameObject panelControlesMobile;

    [Header("Pantalla de la 'realidad'")]
    [Tooltip("Image que cubre toda la pantalla. El color lo elegís vos (blanco = despertar, negro = corte). El alpha lo maneja el script.")]
    [SerializeField] private Image pantalla;
    [SerializeField] private TMP_Text texto;
    [SerializeField] private float segundosFadeEntrada = 3f;
    [Tooltip("Pausa con la pantalla ya cubierta, antes de la primera línea.")]
    [SerializeField] private float pausaTrasFade = 1.2f;

    [Header("Texto")]
    [SerializeField, TextArea(2, 4)]
    private string[] lineas =
    {
        "...",
        "—Dani. Dani, ¿me escuchás?",
        "Sonó el timbre hace rato. Te quedaste dormida en el banco.",
        "Pero ahora estás acá. Y estás bien.",
        "Cuando quieras, vamos."
    };
    [SerializeField] private float segundosFadeTexto = 1f;
    [SerializeField] private float lecturaMinima = 2f;
    [Tooltip("Segundos extra de lectura por cada letra de la línea.")]
    [SerializeField] private float segundosPorLetra = 0.06f;
    [SerializeField] private float pausaEntreLineas = 0.6f;
    [Tooltip("Espera con la pantalla vacía antes de cargar los créditos.")]
    [SerializeField] private float esperaFinal = 2f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("El timbre de la escuela, por ejemplo.")]
    [SerializeField] private AudioClip sfxDespertar;
    [Tooltip("Ambiente, música, niebla... lo que quieras que se apague mientras la pantalla se cubre.")]
    [SerializeField] private AudioSource[] audiosAAtenuar;

    [Header("Escena siguiente")]
    [Tooltip("Tiene que estar agregada en File > Build Settings.")]
    [SerializeField] private string escenaCreditos = "Creditos";
    [Tooltip("Si la escena de créditos no está en el build, se carga esta para no dejar el juego trabado.")]
    [SerializeField] private string escenaRespaldo = "MenuInicio";

    [Header("Eventos (opcional)")]
    [Tooltip("Se dispara cuando la pantalla ya está cubierta del todo (ideal para apagar fantasmas, el fragmento, etc.).")]
    [SerializeField] private UnityEvent alCubrirPantalla;

    private bool _iniciado;

    private void Awake()
    {
        if (jugador == null) jugador = FindFirstObjectByType<ControladorPersonaje>();

        if (jugador == null)
            Debug.LogError("FinalDelJuego: no encontré a Dani en la escena.", this);

        if (pantalla == null || texto == null)
            Debug.LogError("FinalDelJuego: falta asignar la Image 'Pantalla' o el 'Texto'.", this);

        // Todo transparente al arrancar y sin bloquear clics
        FijarAlphaPantalla(0f);
        if (pantalla != null) pantalla.raycastTarget = false;

        if (texto != null)
        {
            texto.text = "";
            texto.alpha = 0f;
        }
    }

    // Enganchar en el "Al Terminar" del mensaje de la maestra
    public void Iniciar()
    {
        if (_iniciado) return;
        _iniciado = true;

        StartCoroutine(Secuencia());
    }

    [ContextMenu("Probar final (solo en Play Mode)")]
    private void Probar()
    {
        if (Application.isPlaying) Iniciar();
    }

    private IEnumerator Secuencia()
    {
        BloquearDani();

        if (audioSource != null && sfxDespertar != null) audioSource.PlayOneShot(sfxDespertar);

        // Anotamos el volumen de cada audio para bajarlo proporcionalmente
        float[] volumenes = new float[audiosAAtenuar != null ? audiosAAtenuar.Length : 0];
        for (int i = 0; i < volumenes.Length; i++)
            volumenes[i] = audiosAAtenuar[i] != null ? audiosAAtenuar[i].volume : 0f;

        if (pantalla != null) pantalla.raycastTarget = true; // desde acá nada se puede tocar

        // 1. La pantalla se cubre y el ambiente se apaga
        yield return Fundir(k =>
        {
            FijarAlphaPantalla(k);
            for (int i = 0; i < volumenes.Length; i++)
                if (audiosAAtenuar[i] != null) audiosAAtenuar[i].volume = volumenes[i] * (1f - k);
        }, 0f, 1f, segundosFadeEntrada);

        alCubrirPantalla?.Invoke();
        yield return new WaitForSecondsRealtime(pausaTrasFade);

        // 2. El texto, línea por línea
        if (texto != null && lineas != null)
        {
            foreach (string linea in lineas)
            {
                if (string.IsNullOrEmpty(linea)) continue;

                texto.text = linea;
                texto.alpha = 0f;

                yield return Fundir(k => texto.alpha = k, 0f, 1f, segundosFadeTexto);
                yield return new WaitForSecondsRealtime(Mathf.Max(lecturaMinima, linea.Length * segundosPorLetra));
                yield return Fundir(k => texto.alpha = k, 1f, 0f, segundosFadeTexto);

                yield return new WaitForSecondsRealtime(pausaEntreLineas);
            }

            texto.text = "";
        }

        yield return new WaitForSecondsRealtime(esperaFinal);

        // 3. Créditos
        CargarEscena();
    }

    // Mismo criterio que el mensaje de los fantasmas: Dani congelada, sin mochila ni controles táctiles.
    // Hace falta repetirlo porque el mensaje de la maestra le devuelve el control justo antes de llamarnos.
    private void BloquearDani()
    {
        if (jugador != null)
        {
            jugador.CambiarEstado(EstadoPersonaje.EN_FLASHBACK);

            if (jugador.movimiento != null) jugador.movimiento.Bloquear(true);

            GestorEntradas entradas = jugador.entradas;
            if (entradas != null)
            {
                entradas.entradaMovimiento = Vector2.zero;
                entradas.EstablecerMovimientoMobile(Vector2.zero);
                entradas.enabled = false;
            }
        }

        if (interfazMochila != null) interfazMochila.Bloquear(true);
        if (panelControlesMobile != null) panelControlesMobile.SetActive(false);
    }

    private void CargarEscena()
    {
        // Por si algún día se pausa el juego con timeScale = 0: la escena siguiente tiene que arrancar normal
        Time.timeScale = 1f;

        // Los créditos y el menú necesitan el mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        string destino = escenaCreditos;

        if (!Application.CanStreamedLevelBeLoaded(destino))
        {
            Debug.LogError($"FinalDelJuego: la escena '{destino}' no está en Build Settings (o el nombre no coincide). Se carga '{escenaRespaldo}'.", this);
            destino = escenaRespaldo;

            if (!Application.CanStreamedLevelBeLoaded(destino))
            {
                Debug.LogError($"FinalDelJuego: tampoco existe '{destino}'. No hay a dónde ir.", this);
                return;
            }
        }

        SceneManager.LoadScene(destino);
    }

    // Interpola con curva suave, con tiempo sin escala (no depende de timeScale)
    private IEnumerator Fundir(Action<float> aplicar, float desde, float hasta, float duracion)
    {
        if (duracion <= 0f)
        {
            aplicar(hasta);
            yield break;
        }

        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duracion));
            aplicar(Mathf.Lerp(desde, hasta, k));
            yield return null;
        }

        aplicar(hasta);
    }

    private void FijarAlphaPantalla(float alpha)
    {
        if (pantalla == null) return;

        Color c = pantalla.color;
        c.a = alpha;
        pantalla.color = c;
    }
}