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
    [SerializeField] private Button botonUsar;
    [SerializeField] private Button botonDescartar;
    [Tooltip("Opcional: muestra avisos como 'La linterna ya está cargada'.")]
    [SerializeField] private TMP_Text textoMensaje;

    private SlotMochilaUI[] _slots;
    private int _indiceSeleccionado = -1;
    private CursorLockMode _cursorPrevio;
    private bool _cursorVisiblePrevio;

    public bool EstaAbierta => panel.activeSelf;

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
        if (botonUsar != null) botonUsar.onClick.AddListener(UsarSeleccionado);

        panel.SetActive(false);
    }

    private void OnEnable()
    {
        mochila.OnMochilaCambiada += Refrescar;
        Refrescar();
    }

    private void OnDisable()
    {
        mochila.OnMochilaCambiada -= Refrescar;
    }

    // Lo llama AtajoMochila (o GestorEntradas cuando lo muevas)
    public void Alternar()
    {
        bool abrir = !panel.activeSelf;

        if (abrir && jugador.estado == EstadoPersonaje.EN_FLASHBACK) return;

        panel.SetActive(abrir);

        // Solo se bloquea a Dani: el mundo SIGUE corriendo con la mochila abierta (decisión de diseño)
        jugador.movimiento.Bloquear(abrir);

        if (abrir)
        {
            _cursorPrevio = Cursor.lockState;
            _cursorVisiblePrevio = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _indiceSeleccionado = -1;
            ActualizarDetalle();
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

        // "Usar" solo está activo si el ítem tiene un efecto (las llaves no: se usan en la puerta)
        if (botonUsar != null) botonUsar.interactable = hayItem && item.efecto != null;

        if (textoMensaje != null) textoMensaje.text = "";
    }

    private void UsarSeleccionado()
    {
        if (_indiceSeleccionado < 0) return;

        int cantidadAntes = mochila.Items.Count;
        mochila.UsarDesdeMochila(_indiceSeleccionado, jugador, out string mensaje);

        // Si se consumió, los ítems de abajo se corrieron un lugar: la selección ya no apunta a lo mismo
        if (mochila.Items.Count != cantidadAntes)
        {
            _indiceSeleccionado = -1;
            ActualizarDetalle();
        }

        if (textoMensaje != null) textoMensaje.text = mensaje;
    }

    private void DescartarSeleccionado()
    {
        if (_indiceSeleccionado < 0) return;

        int indice = _indiceSeleccionado;
        _indiceSeleccionado = -1;
        mochila.Descartar(indice);
        ActualizarDetalle();
    }
}