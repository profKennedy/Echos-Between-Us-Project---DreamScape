using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Escena de créditos: scroll vertical sobre fondo negro, y al final un mensaje del equipo.
//
// Secuencia:
//   1. Los créditos suben desde abajo de la pantalla hasta salir por arriba.
//      (Mantener clic / tocar / Espacio / Flecha abajo = acelerar. Escape = saltar directo al mensaje.)
//   2. Una pausa, y aparece el mensaje del equipo, página por página, con fade.
//      El mensaje NO se puede saltar: es lo más importante de la escena.
//   3. La última página se queda unos segundos más, la música baja y se vuelve al menú.
//
// ESTRUCTURA en la Hierarchy:
//   Canvas (Overlay, Scale With Screen Size 1920x1080)
//     ├─ Fondo      (Image negra, estirada a toda la pantalla)
//     ├─ Contenido  (TextMeshPro + Content Size Fitter vertical = Preferred Size)
//     └─ Mensaje    (TextMeshPro, estirado, centrado)
//   Creditos        (vacío con este script)
//
// AJUSTES del RectTransform de "Contenido" (importante):
//   Anchors = top-center (0.5, 1), Pivot = (0.5, 1), Width fijo (ej. 1400), Alignment = Center.
public class CreditosScroll : MonoBehaviour
{
    [Header("Créditos (scroll vertical)")]
    [Tooltip("El TextMeshPro con todos los créditos. Anchors top-center, Pivot (0.5, 1) y Content Size Fitter vertical.")]
    [SerializeField] private RectTransform contenido;
    [Tooltip("Unidades del Canvas por segundo (con el reference de 1080 de alto, 80-100 se lee cómodo).")]
    [SerializeField] private float velocidad = 90f;
    [SerializeField] private float multiplicadorAcelerado = 4f;
    [SerializeField] private float esperaInicial = 1.5f;

    [Header("Mensaje del equipo")]
    [SerializeField] private TMP_Text mensaje;
    [SerializeField, TextArea(3, 8)]
    private string[] paginas =
    {
        "El bullying no es un juego ni \"cosas de chicos\".\nDeja heridas que no se ven, y muchas veces se cargan en silencio.",
        "Echoes Between Us nació para hablar de eso:\nde sentirse pequeño o pequeña, de callar, de creer que nadie va a entender.",
        "Si lo estás viviendo, no tenés que atravesarlo solo ni sola.\nContáselo a alguien de confianza: un familiar, un docente, un amigo.\nPedir ayuda no te hace débil.",
        "Y si lo ves pasar, no mires para otro lado.\nA veces, una sola persona que se acerca lo cambia todo.",
        "No estás solo. No estás sola.\n\n— DreamScape"
    };
    [SerializeField] private float pausaTrasCreditos = 1.5f;
    [SerializeField] private float segundosFade = 1.5f;
    [SerializeField] private float lecturaMinima = 4f;
    [Tooltip("Segundos extra de lectura por cada letra de la página.")]
    [SerializeField] private float segundosPorLetra = 0.06f;
    [SerializeField] private float pausaEntrePaginas = 0.8f;
    [Tooltip("Tiempo EXTRA que se queda la última página antes de salir.")]
    [SerializeField] private float esperaFinal = 6f;

    [Header("Audio (opcional)")]
    [Tooltip("La música de los créditos: la que baja al final.")]
    [SerializeField] private AudioSource musica;

    [Header("Salida")]
    [SerializeField] private float segundosFadeSalida = 2f;
    [SerializeField] private string escenaSiguiente = "MenuInicio";

    private bool _saltarCreditos;

    private void Awake()
    {
        if (mensaje != null)
        {
            mensaje.text = "";
            mensaje.alpha = 0f;
        }

        if (contenido == null || mensaje == null)
            Debug.LogError("CreditosScroll: falta asignar 'Contenido' o 'Mensaje'.", this);
    }

    private void Start()
    {
        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        if (contenido == null || mensaje == null) yield break;

        // El Content Size Fitter calcula el alto recién después de un frame
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contenido);

        if (contenido.pivot.y < 0.99f || contenido.anchorMin.y < 0.99f || contenido.anchorMax.y < 0.99f)
            Debug.LogWarning("CreditosScroll: 'Contenido' tiene que tener Anchors arriba (top) y Pivot Y = 1 para que el scroll quede bien.", this);

        RectTransform area = contenido.parent as RectTransform;
        float altoPantalla = area != null ? area.rect.height : 1080f;
        float altoContenido = contenido.rect.height;

        // Arranca justo debajo de la pantalla y termina cuando el final del texto salió por arriba
        float y = -altoPantalla;
        float yFinal = altoContenido;
        contenido.anchoredPosition = new Vector2(contenido.anchoredPosition.x, y);

        yield return Esperar(esperaInicial);

        // 1. Créditos
        while (y < yFinal && !_saltarCreditos)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) _saltarCreditos = true;

            float multiplicador = Acelerando() ? multiplicadorAcelerado : 1f;
            y += velocidad * multiplicador * Time.unscaledDeltaTime;
            contenido.anchoredPosition = new Vector2(contenido.anchoredPosition.x, y);

            yield return null;
        }

        contenido.gameObject.SetActive(false);
        yield return Esperar(pausaTrasCreditos);

        // 2. Mensaje del equipo (sin saltar)
        for (int i = 0; i < paginas.Length; i++)
        {
            string pagina = paginas[i];
            if (string.IsNullOrEmpty(pagina)) continue;

            mensaje.text = pagina;
            yield return Fundir(k => mensaje.alpha = k, 0f, 1f, segundosFade);

            float lectura = Mathf.Max(lecturaMinima, pagina.Length * segundosPorLetra);
            bool esUltima = i == paginas.Length - 1;
            if (esUltima) lectura += esperaFinal;

            yield return Esperar(lectura);

            // La última no se apaga sola: se va con la música y la salida
            if (!esUltima)
            {
                yield return Fundir(k => mensaje.alpha = k, 1f, 0f, segundosFade);
                yield return Esperar(pausaEntrePaginas);
            }
        }

        // 3. Salida: bajan juntos el texto y la música
        float volumenInicial = musica != null ? musica.volume : 0f;
        yield return Fundir(k =>
        {
            mensaje.alpha = 1f - k;
            if (musica != null) musica.volume = volumenInicial * (1f - k);
        }, 0f, 1f, segundosFadeSalida);

        CargarEscena();
    }

    private static bool Acelerando()
    {
        return Input.GetMouseButton(0) || Input.touchCount > 0 ||
               Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.DownArrow);
    }

    private void CargarEscena()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!Application.CanStreamedLevelBeLoaded(escenaSiguiente))
        {
            Debug.LogError($"CreditosScroll: la escena '{escenaSiguiente}' no está en Build Settings.", this);
            return;
        }

        SceneManager.LoadScene(escenaSiguiente);
    }

    private static IEnumerator Esperar(float segundos)
    {
        float t = 0f;
        while (t < segundos)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    // Interpola con curva suave y tiempo sin escala
    private static IEnumerator Fundir(Action<float> aplicar, float desde, float hasta, float duracion)
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
}