using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PuzzleSecuencia : MonoBehaviour
{
    [Header("Objetos del puzzle")]
    [SerializeField] private DatosItem pelota;
    [SerializeField] private DatosItem cinta;
    [SerializeField] private DatosItem fotografia;

    [Header("Referencias")]
    [SerializeField] private MochilaDani mochila;
    [SerializeField] private TMP_Text textoResultado;

    [Header("Espacios de secuencia")]
    [SerializeField] private TMP_Text espacio1;
    [SerializeField] private TMP_Text espacio2;
    [SerializeField] private TMP_Text espacio3;

    [Header("Eventos")]
    public UnityEvent alCompletarPuzzle;

    private DatosItem[] secuenciaCorrecta;
    private DatosItem[] secuenciaJugador;

    private int posicionActual;
    private bool completado;

    public bool EstaCompletado => completado;

    private void Awake()
    {
        secuenciaCorrecta = new DatosItem[]
        {
            pelota,
            cinta,
            fotografia
        };

        secuenciaJugador = new DatosItem[3];

        ReiniciarInterfaz();
    }

    public void SeleccionarPelota()
    {
        SeleccionarObjeto(pelota);
    }

    public void SeleccionarCinta()
    {
        SeleccionarObjeto(cinta);
    }

    public void SeleccionarFotografia()
    {
        SeleccionarObjeto(fotografia);
    }

    private void SeleccionarObjeto(DatosItem item)
    {
        if (completado || posicionActual >= 3)
            return;

        if (item == null || mochila == null)
            return;

        if (!mochila.Contiene(item))
        {
            textoResultado.text = "Todavia no encontraste este objeto.";
            return;
        }

        // Evita colocar el mismo objeto dos veces.
        for (int i = 0; i < posicionActual; i++)
        {
            if (secuenciaJugador[i] == item)
                return;
        }

        secuenciaJugador[posicionActual] = item;
        posicionActual++;

        ActualizarInterfaz();

        if (posicionActual == 3)
            ComprobarSecuencia();
    }

    private void ComprobarSecuencia()
    {
        for (int i = 0; i < 3; i++)
        {
            if (secuenciaJugador[i] != secuenciaCorrecta[i])
            {
                textoResultado.text = "El orden no es correcto.";

                Invoke(nameof(ReiniciarInterfaz), 2f);
                return;
            }
        }

        completado = true;

        mochila.TryQuitar(pelota);
        mochila.TryQuitar(cinta);
        mochila.TryQuitar(fotografia);

        textoResultado.text = "Has reconstruido el recuerdo.";

        Debug.Log("Puzzle del gimnasio completado.");

        alCompletarPuzzle?.Invoke();
    }

    private void ActualizarInterfaz()
    {
        espacio1.text = secuenciaJugador[0] != null
            ? secuenciaJugador[0].nombre : "?";

        espacio2.text = secuenciaJugador[1] != null
            ? secuenciaJugador[1].nombre : "?";

        espacio3.text = secuenciaJugador[2] != null
            ? secuenciaJugador[2].nombre : "?";
    }

    private void ReiniciarInterfaz()
    {
        posicionActual = 0;

        secuenciaJugador = new DatosItem[3];

        espacio1.text = "?";
        espacio2.text = "?";
        espacio3.text = "?";

        textoResultado.text = "";
    }
}