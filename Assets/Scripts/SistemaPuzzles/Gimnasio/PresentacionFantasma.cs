using System.Collections;
using UnityEngine;
using Cinemachine;

public class PresentacionFantasma : MonoBehaviour
{
    [Header("Controles Mobile")]
    [SerializeField] private GameObject panelControlesMobile;

    private bool controlesMobileEstabanActivos;

    [Header("Camaras")]
    [SerializeField] private CinemachineVirtualCamera camaraFantasma;
    [SerializeField] private CinemachineFreeLook camaraDani;

    [Header("Fantasma")]
    [SerializeField] private GameObject fantasma;

    [Header("Jugador")]
    [SerializeField] private ControladorPersonaje jugador;
    [SerializeField] private GestorEntradas gestorEntradas;
    [SerializeField] private InterfazMochila interfazMochila;

    [Header("Audio")]
    [SerializeField] private AudioSource sonidoPresentacion;

    [Header("Configuracion")]
    [SerializeField] private float duracion = 4f;

    private bool presentada = false;

    private float velocidadXAnterior;
    private float velocidadYAnterior;

    private bool entradasEstabanActivas;

    public void IniciarPresentacion()
    {
        if (presentada)
            return;

        presentada = true;

        StartCoroutine(SecuenciaPresentacion());
    }

    private IEnumerator SecuenciaPresentacion()
    {
        // 1. Bloquear movimiento de Dani.
        if (jugador != null)
            jugador.movimiento.Bloquear(true);

        // 2. Bloquear mochila.
        if (interfazMochila != null)
            interfazMochila.Bloquear(true);

        // Guardamos el estado original de los controles móviles.
        if (panelControlesMobile != null)
        {
            controlesMobileEstabanActivos =
                panelControlesMobile.activeSelf;

            // Ocultamos y desactivamos los controles.
            panelControlesMobile.SetActive(false);
        }

        // 3. Desactivar temporalmente el gestor de entradas.
        if (gestorEntradas != null)
        {
            gestorEntradas.entradaMovimiento = Vector2.zero;
            gestorEntradas.EstablecerMovimientoMobile(Vector2.zero);

            entradasEstabanActivas = gestorEntradas.enabled;
            gestorEntradas.enabled = false;
        }

        // 4. Bloquear los ejes de la camara FreeLook.
        if (camaraDani != null)
        {
            velocidadXAnterior = camaraDani.m_XAxis.m_MaxSpeed;
            velocidadYAnterior = camaraDani.m_YAxis.m_MaxSpeed;

            camaraDani.m_XAxis.m_MaxSpeed = 0f;
            camaraDani.m_YAxis.m_MaxSpeed = 0f;
        }

        // 5. Activar el fantasma.
        if (fantasma != null)
            fantasma.SetActive(true);

        // 6. Cambiar a la camara de presentacion.
        if (camaraFantasma != null)
            camaraFantasma.Priority = 20;

        // 7. Reproducir sonido.
        if (sonidoPresentacion != null)
            sonidoPresentacion.Play();

        // 8. Esperar cuatro segundos.
        yield return new WaitForSeconds(duracion);

        // 9. Regresar a la camara de Dani.
        if (camaraFantasma != null)
            camaraFantasma.Priority = 5;

        // Esperar a que Cinemachine realice la transicion.
        yield return new WaitForSeconds(0.5f);

        // 10. Restaurar los ejes de la camara.
        if (camaraDani != null)
        {
            camaraDani.m_XAxis.m_MaxSpeed = velocidadXAnterior;
            camaraDani.m_YAxis.m_MaxSpeed = velocidadYAnterior;
        }

        // 11. Restaurar las entradas.
        if (gestorEntradas != null)
            gestorEntradas.enabled = entradasEstabanActivas;

        // Restauramos el estado original.
        if (panelControlesMobile != null)
        {
            panelControlesMobile.SetActive(
                controlesMobileEstabanActivos
            );
        }

        // 12. Desbloquear movimiento.
        if (jugador != null)
            jugador.movimiento.Bloquear(false);

        // 13. Desbloquear mochila.
        if (interfazMochila != null)
            interfazMochila.Bloquear(false);
    }
}