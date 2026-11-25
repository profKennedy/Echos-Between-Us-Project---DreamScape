using UnityEngine;
using UnityEngine.UI;

public class ConfirmarGuardarOpciones : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelOpciones;
    public GameObject panelConfirmacion;
    public GameObject panelAnterior; // Menú Principal o Menú de Pausa

    [Header("Sliders a controlar")]
    public Slider sliderVolumen;
    public Slider sliderBrillo;
    public Slider sliderJoystick; // Opcional (si querés que también lo revierta)

    private float volInicial;
    private float brilloInicial;
    private float joystickInicial;

    private void OnEnable()
    {
        // Guarda cómo estaban los valores al abrir Opciones
        if (sliderVolumen != null) volInicial = sliderVolumen.value;
        if (sliderBrillo != null) brilloInicial = sliderBrillo.value;
        if (sliderJoystick != null) joystickInicial = sliderJoystick.value;
    }

    // Conectá esto al botón "Volver / Salir" de Opciones
    public void IntentarSalir()
    {
        panelConfirmacion.SetActive(true);
    }

    // Conectá esto al botón "SÍ"
    public void ConfirmarSi()
    {
        PlayerPrefs.Save();
        CerrarTodo();
    }

    // Conectá esto al botón "NO"
    public void ConfirmarNo()
    {
        if (sliderVolumen != null) sliderVolumen.value = volInicial;
        if (sliderBrillo != null) sliderBrillo.value = brilloInicial;
        if (sliderJoystick != null) sliderJoystick.value = joystickInicial;

        PlayerPrefs.Save();
        panelConfirmacion.SetActive(false);
    }

    private void CerrarTodo()
    {
        panelConfirmacion.SetActive(false);
        panelOpciones.SetActive(false);
        if (panelAnterior != null) panelAnterior.SetActive(true);
    }
}