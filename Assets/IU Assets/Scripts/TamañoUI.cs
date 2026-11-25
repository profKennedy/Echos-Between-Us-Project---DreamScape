using UnityEngine;
using UnityEngine.UI;
using System;

public class ControladorTamanoUI : MonoBehaviour
{
    // Evento para avisar en vivo al joystick y botones si estamos dentro del juego
    public static event Action<float> OnEscalaCambiada;
    public const string CLAVE_ESCALA = "EscalaControles";

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        // Carga el valor guardado (por defecto 1 = tamaño normal)
        float escalaGuardada = PlayerPrefs.GetFloat(CLAVE_ESCALA, 1f);
        slider.SetValueWithoutNotify(escalaGuardada);

        // Conecta el evento automáticamente sin tener que hacerlo a mano en el Inspector
        slider.onValueChanged.AddListener(CambiarEscala);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(CambiarEscala);
    }

    private void CambiarEscala(float nuevoValor)
    {
        PlayerPrefs.SetFloat(CLAVE_ESCALA, nuevoValor);
        PlayerPrefs.Save();

        // Avisa a todos los controles activos que actualicen su tamaño
        OnEscalaCambiada?.Invoke(nuevoValor);
    }
}