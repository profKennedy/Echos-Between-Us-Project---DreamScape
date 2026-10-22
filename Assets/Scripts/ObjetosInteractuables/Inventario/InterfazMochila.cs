using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Poné este script en el Canvas (que siempre está activo), NO en el panel que se oculta.
public class InterfazMochila : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MochilaDani mochila;
    [SerializeField] private ControladorPersonaje jugador;
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform contenedorSlots;   // el objeto con el Grid Layout Group
    [SerializeField] private SlotMochilaUI prefabSlot;

    [Header("Detalle del item seleccionado")]
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoDescripcion;
    [SerializeField] private Button botonUsar;            // también hace de "Equipar", según el item
    [SerializeField] private Button botonDescartar;

    [Header("Mano")]
    [SerializeField] private ManoDani mano;
    [SerializeField] private Image iconoMano;
    [SerializeField] private Button botonGuardarMano;

    [Header("Mensajes")]
    [Tooltip("Ponelo DENTRO del PanelMochila. Opcional.")]
    [SerializeField] private TMP_Text textoMensaje;
    [SerializeField] private float segundosVisible = 1f;
    [SerializeField] private float segundosDesvanecer = 0.4f;

    private SlotMochilaUI[] _slots;
    private TMP_Text _textoBotonUsar;
    private Coroutine _rutinaMensaje;
    private int _indiceSeleccionado = -1;
    private CursorLockMode _cursorPrevio;
    private bool _cursorVisiblePrevio;
    private bool _bloqueada;

    public bool EstaAbierta => panel.activeSelf;

    public void Bloquear(bool valor)
    {
        _bloqueada = valor;
        if (valor && panel.activeSelf) Alternar(); // si estaba abierta, se cierra
    }

    private void Awake()
    {
        _slots = new SlotMochilaUI[mochila.Capacidad];

        for (int i = 0; i < _slots.Length; i++)
        {
            SlotMochilaUI slot = Instantiate(prefabSlot, contenedorSlots);
            slot.Inicializar(i);
            slot.OnSlotSeleccionado += Seleccionar;
            _slots[i] = slot;
        }

        botonDescartar.onClick.AddListener(DescartarSeleccionado);

        if (botonUsar != null)
        {
            botonUsar.onClick.AddListener(UsarSeleccionado);
            _textoBotonUsar = botonUsar.GetComponentInChildren<TMP_Text>();
        }

        if (botonGuardarMano != null) botonGuardarMano.onClick.AddListener(GuardarMano);

        // Los textos/iconos decorativos no deben tapar los clics de los botones
        if (iconoMano != null) iconoMano.raycastTarget = false;
        if (textoMensaje != null) textoMensaje.raycastTarget = false;

        panel.SetActive(false);
    }

    private void OnEnable()
    {
        mochila.OnMochilaCambiada += Refrescar;
        if (mano != null) mano.OnManoCambiada += RefrescarMano;

        Refrescar();
        RefrescarMano();
    }

    private void OnDisable()
    {
        mochila.OnMochilaCambiada -= Refrescar;
        if (mano != null) mano.OnManoCambiada -= RefrescarMano;

        CancelarMensaje();
    }

    // Lo llama AtajoMochila (o GestorEntradas cuando lo muevas)
    public void Alternar()
    {
        bool abrir = !panel.activeSelf;

        if (abrir && (_bloqueada || jugador.estado == EstadoPersonaje.EN_FLASHBACK)) return;

        CancelarMensaje();
        panel.SetActive(abrir);

        // Solo se bloquea a Dani: el mundo SIGUE corriendo con la mochila abierta 
        jugador.movimiento.Bloquear(abrir);

        if (abrir)
        {
            _cursorPrevio = Cursor.lockState;
            _cursorVisiblePrevio = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _indiceSeleccionado = -1;
            ActualizarDetalle();
            RefrescarMano();
        }
        else
        {
            Cursor.lockState = _cursorPrevio;
            Cursor.visible = _cursorVisiblePrevio;
        }
    }

    private void Refrescar()
    {
        for (int i = 0; i < _slots.Length; i++)
            _slots[i].Mostrar(i < mochila.Items.Count ? mochila.Items[i] : null);

        if (_indiceSeleccionado >= mochila.Items.Count) _indiceSeleccionado = -1;
        ActualizarDetalle();
    }

    private void RefrescarMano()
    {
        if (mano == null) return;

        DatosItem item = mano.ItemActual;
        bool hayItem = item != null;

        if (iconoMano != null)
        {
            iconoMano.enabled = hayItem;
            iconoMano.sprite = hayItem ? item.icono : null;
        }

        if (botonGuardarMano != null) botonGuardarMano.interactable = hayItem;
    }

    private void Seleccionar(int indice)
    {
        _indiceSeleccionado = indice < mochila.Items.Count ? indice : -1;
        ActualizarDetalle();
    }

    private void ActualizarDetalle()
    {
        bool hayItem = _indiceSeleccionado >= 0 && _indiceSeleccionado < mochila.Items.Count;
        DatosItem item = hayItem ? mochila.Items[_indiceSeleccionado] : null;

        textoNombre.text = hayItem ? item.nombre : "";
        textoDescripcion.text = hayItem ? item.descripcion : "";
        botonDescartar.interactable = hayItem;

        // El botón de acción solo está activo si el ítem tiene un efecto (las llaves no: se usan en la puerta)
        if (botonUsar != null)
        {
            botonUsar.interactable = hayItem && item.efecto != null;

            if (_textoBotonUsar != null)
                _textoBotonUsar.text = hayItem && !string.IsNullOrEmpty(item.textoAccion) ? item.textoAccion : "Usar";
        }
    }

    private void UsarSeleccionado()
    {
        if (_indiceSeleccionado < 0) return;

        int cantidadAntes = mochila.Items.Count;
        mochila.UsarDesdeMochila(_indiceSeleccionado, jugador, out string mensaje);

        // Si salió un ítem de la lista, los de abajo se corrieron un lugar: la selección ya no apunta a lo mismo
        if (mochila.Items.Count != cantidadAntes)
        {
            _indiceSeleccionado = -1;
            ActualizarDetalle();
        }

        MostrarMensaje(mensaje);
    }

    private void GuardarMano()
    {
        if (mano == null) return;

        mano.TryGuardarEnMochila(mochila, out string mensaje);
        MostrarMensaje(mensaje);
    }

    private void DescartarSeleccionado()
    {
        if (_indiceSeleccionado < 0) return;

        int indice = _indiceSeleccionado;
        _indiceSeleccionado = -1;
        mochila.Descartar(indice);
        ActualizarDetalle();
    }

    // ---------------------------------------------------------------
    // Mensajes: aparecen, quedan 'segundosVisible' y se desvanecen solos
    // ---------------------------------------------------------------
    private void MostrarMensaje(string mensaje)
    {
        if (textoMensaje == null) return;

        CancelarMensaje();
        if (string.IsNullOrEmpty(mensaje)) return;

        _rutinaMensaje = StartCoroutine(RutinaMensaje(mensaje));
    }

    private IEnumerator RutinaMensaje(string mensaje)
    {
        textoMensaje.text = mensaje;
        SetAlfa(1f);

        yield return new WaitForSecondsRealtime(segundosVisible);

        float transcurrido = 0f;
        while (transcurrido < segundosDesvanecer)
        {
            transcurrido += Time.unscaledDeltaTime;
            SetAlfa(1f - Mathf.Clamp01(transcurrido / segundosDesvanecer));
            yield return null;
        }

        textoMensaje.text = "";
        SetAlfa(1f);
        _rutinaMensaje = null;
    }

    private void CancelarMensaje()
    {
        if (_rutinaMensaje != null)
        {
            StopCoroutine(_rutinaMensaje);
            _rutinaMensaje = null;
        }

        if (textoMensaje != null)
        {
            textoMensaje.text = "";
            SetAlfa(1f);
        }
    }

    private void SetAlfa(float alfa)
    {
        Color color = textoMensaje.color;
        color.a = alfa;
        textoMensaje.color = color;
    }
}