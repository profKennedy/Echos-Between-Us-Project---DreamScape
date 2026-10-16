
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickMobile : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform fondo;
    [SerializeField] private RectTransform palanca;
    [SerializeField] private GestorEntradas entradas;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);

    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 posicionLocal;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            fondo,
            eventData.position,
            eventData.pressEventCamera,
            out posicionLocal))
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
        palanca.anchoredPosition = Vector2.zero;
        entradas.EstablecerMovimientoMobile(Vector2.zero);
    }

    private void OnDisable()
    {
        if (palanca != null)
            palanca.anchoredPosition = Vector2.zero;

        if (entradas != null)
            entradas.EstablecerMovimientoMobile(Vector2.zero);
    }
}
