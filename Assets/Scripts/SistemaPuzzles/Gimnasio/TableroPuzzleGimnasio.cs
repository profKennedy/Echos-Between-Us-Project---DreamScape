using UnityEngine;
using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;

public class TableroPuzzleGimnasio : MonoBehaviour, IInteractable
{
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

        panelPuzzle.SetActive(true);

        // Bloqueamos el movimiento de Dani.
        jugadorActual.movimiento.Bloquear(true);

        // Guardamos el estado anterior del cursor.
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

        if (jugadorActual != null)
            jugadorActual.movimiento.Bloquear(false);

        Cursor.lockState = cursorAnterior;
        Cursor.visible = cursorVisibleAnterior;

        jugadorActual = null;
    }
}