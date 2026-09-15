
using UnityEngine;
using UnityEngine.UI;

public class VolumenController : MonoBehaviour
{
    public Slider slider;
    public float sliderValue;

    void Start()
    {
        slider.value = PlayerPrefs.GetFloat("volumenAudio",0.5f);
        AudioListener.volume = slider.value;
    }

    public void ChangeSlider(float valor)
    {
        sliderValue = valor;
        PlayerPrefs.SetFloat("volumenAudio", sliderValue);
        AudioListener.volume = slider.value;

    }

}
