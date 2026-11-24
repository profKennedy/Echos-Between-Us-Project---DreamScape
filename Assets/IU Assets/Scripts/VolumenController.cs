// VolumenController.cs
using UnityEngine;
using UnityEngine.UI;

public class VolumenController : MonoBehaviour
{
    private const string ClaveVolumen = "volumenAudio";

    [SerializeField] private Slider slider;

    private void Start()
    {
        float guardado = PlayerPrefs.GetFloat(ClaveVolumen, 0.5f);
        AudioListener.volume = guardado;

        if (slider != null) slider.SetValueWithoutNotify(guardado);
        else Debug.LogError("VolumenController: falta asignar 'Slider'.", this);
    }

    public void ChangeSlider(float valor)
    {
        PlayerPrefs.SetFloat(ClaveVolumen, valor);
        AudioListener.volume = valor;
    }
}