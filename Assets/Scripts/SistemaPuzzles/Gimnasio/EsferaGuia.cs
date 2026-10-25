using UnityEngine;

public class EsferaGuia : MonoBehaviour
{
    [Header("Movimiento de flotacion")]
    [SerializeField] private float amplitud = 0.2f;
    [SerializeField] private float velocidadFlotacion = 2f;

    [Header("Deteccion de Dani")]
    [SerializeField] private Transform jugador;
    [SerializeField] private float distanciaActivacion = 2.5f;

    [Header("Movimiento hacia el gimnasio")]
    [SerializeField] private Transform puntoDestino;
    [SerializeField] private float velocidadMovimiento = 2f;

    [Header("Desaparicion")]
    [SerializeField] private float distanciaDesaparicion = 0.15f;

    private Vector3 posicionBase;
    private bool guiando = false;
    private bool terminado = false;

    private void Start()
    {
        posicionBase = transform.position;
    }

    private void Update()
    {
        if (terminado)
            return;

        // 1. Detectar si Dani se acerca.
        if (!guiando && jugador != null && puntoDestino != null)
        {
            float distanciaJugador = Vector3.Distance(
                jugador.position,
                transform.position
            );

            if (distanciaJugador <= distanciaActivacion)
            {
                guiando = true;

                Debug.Log("La esfera comienza a guiar a Dani.");
            }
        }

        // 2. Moverse hacia el gimnasio.
        if (guiando && puntoDestino != null)
        {
            posicionBase = Vector3.MoveTowards(
                posicionBase,
                puntoDestino.position,
                velocidadMovimiento * Time.deltaTime
            );

            // 3. Desaparecer al llegar al destino.
            if (Vector3.Distance(posicionBase, puntoDestino.position)
                <= distanciaDesaparicion)
            {
                terminado = true;
                gameObject.SetActive(false);
                return;
            }
        }

        // 4. Mantener la flotacion durante todo el recorrido.
        float desplazamientoVertical =
            Mathf.Sin(Time.time * velocidadFlotacion) * amplitud;

        transform.position = posicionBase +
            Vector3.up * desplazamientoVertical;
    }
}