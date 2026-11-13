
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickMobile : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Referencias")]
    [SerializeField] private RectTransform fondo;
    [SerializeField] private RectTransform palanca;
    [SerializeField] private GestorEntradas entradas;

    // Identifica el dedo que controla este joystick.
    private int dedoActivo = int.MinValue;

    private bool joystickActivo = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Si ya hay un dedo controlando el joystick,
        // ignoramos cualquier otro.
        if (joystickActivo)
            return;

        // Solo aceptamos toques que comiencen
        // dentro del área del joystick.
        if (!RectTransformUtility.RectangleContainsScreenPoint(
            fondo,
            eventData.position,
            eventData.pressEventCamera))
        {
            return;
        }

        dedoActivo = eventData.pointerId;
        joystickActivo = true;

        ActualizarJoystick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Ignorar arrastres de otros dedos.
        if (!joystickActivo)
            return;

        if (eventData.pointerId != dedoActivo)
            return;

        ActualizarJoystick(eventData);
    }

    private void ActualizarJoystick(PointerEventData eventData)
    {
        if (fondo == null || palanca == null || entradas == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            fondo,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 posicionLocal))
        {
            Vector2 radio = fondo.rect.size * 0.5f;

            Vector2 direccion = new Vector2(
                posicionLocal.x / radio.x,
                posicionLocal.y / radio.y
            );

            direccion = Vector2.ClampMagnitude(direccion, 1f);

            palanca.anchoredPosition = new Vector2(
                direccion.x * radio.x,
                direccion.y * radio.y
            );

            entradas.EstablecerMovimientoMobile(direccion);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Solo el dedo que inició el movimiento
        // puede liberar este joystick.
        if (!joystickActivo)
            return;

        if (eventData.pointerId != dedoActivo)
            return;

        ReiniciarJoystick();
    }

    private void ReiniciarJoystick()
    {
        joystickActivo = false;
        dedoActivo = int.MinValue;

        if (palanca != null)
            palanca.anchoredPosition = Vector2.zero;

        if (entradas != null)
            entradas.EstablecerMovimientoMobile(Vector2.zero);
    }

    private void OnDisable()
    {
        ReiniciarJoystick();
    }
}
