using UnityEngine;
using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using Cinemachine;

public class TableroPuzzleGimnasio : MonoBehaviour, IInteractable
{
    [Header("Cámara de Dani")]
    [SerializeField] private CinemachineFreeLook camaraDani;

    private string ejeHorizontalAnterior;
    private string ejeVerticalAnterior;

    [Header("Referencias")]
    [SerializeField] private PuzzleSecuencia puzzle;
    [SerializeField] private GameObject panelPuzzle;
    [SerializeField] private GameObject cartelInteraccion;

    private ControladorPersonaje jugadorActual;

    private CursorLockMode cursorAnterior;
    private bool cursorVisibleAnterior;

    private bool abierto;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (puzzle == null || panelPuzzle == null || jugador == null)
            return;

        if (puzzle.EstaCompletado || abierto)
            return;

        jugadorActual = jugador;

        AbrirPanel();
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelInteraccion != null)
        {
            cartelInteraccion.SetActive(
                mostrar && !abierto && puzzle != null && !puzzle.EstaCompletado
            );
        }
    }

    private void AbrirPanel()
    {
        abierto = true;

        // Limpiar mensajes anteriores del puzzle.
        puzzle.LimpiarMensaje();

        panelPuzzle.SetActive(true);

        // Bloquear el movimiento de Dani.
        jugadorActual.movimiento.Bloquear(true);

        // Desactivar el control del mouse sobre la cámara.
        if (camaraDani != null)
        {
            ejeHorizontalAnterior = camaraDani.m_XAxis.m_InputAxisName;
            ejeVerticalAnterior = camaraDani.m_YAxis.m_InputAxisName;

            camaraDani.m_XAxis.m_InputAxisName = "";
            camaraDani.m_YAxis.m_InputAxisName = "";

            camaraDani.m_XAxis.m_InputAxisValue = 0f;
            camaraDani.m_YAxis.m_InputAxisValue = 0f;
        }

        // Guardar el estado anterior del cursor.
        cursorAnterior = Cursor.lockState;
        cursorVisibleAnterior = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        MostrarPista(false);
    }

    public void CerrarPanel()
    {
        if (!abierto)
            return;

        abierto = false;

        panelPuzzle.SetActive(false);

        // Restaurar el control de la cámara.
        if (camaraDani != null)
        {
            camaraDani.m_XAxis.m_InputAxisName = ejeHorizontalAnterior;
            camaraDani.m_YAxis.m_InputAxisName = ejeVerticalAnterior;

            camaraDani.m_XAxis.m_InputAxisValue = 0f;
            camaraDani.m_YAxis.m_InputAxisValue = 0f;
        }

        if (jugadorActual != null)
            jugadorActual.movimiento.Bloquear(false);

        Cursor.lockState = cursorAnterior;
        Cursor.visible = cursorVisibleAnterior;

        jugadorActual = null;
    }
}