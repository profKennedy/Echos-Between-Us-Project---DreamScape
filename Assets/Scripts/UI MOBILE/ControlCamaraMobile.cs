
using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class ControlCamaraMobile : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private CinemachineFreeLook camaraDani;

    [Header("Sensibilidad táctil")]
    [SerializeField] private float sensibilidadX = 0.15f;
    [SerializeField] private float sensibilidadY = 0.002f;

    private bool arrastrando;
    private int dedoActivo = -1;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (arrastrando) return;

        arrastrando = true;
        dedoActivo = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastrando || eventData.pointerId != dedoActivo)
            return;

        if (camaraDani == null) return;

        Vector2 delta = eventData.delta;

        // Rotación horizontal.
        camaraDani.m_XAxis.Value += delta.x * sensibilidadX;

        // Movimiento vertical entre órbitas.
        camaraDani.m_YAxis.Value -= delta.y * sensibilidadY;
        camaraDani.m_YAxis.Value =
            Mathf.Clamp01(camaraDani.m_YAxis.Value);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != dedoActivo) return;

        arrastrando = false;

        dedoActivo = -1;
    }
}
