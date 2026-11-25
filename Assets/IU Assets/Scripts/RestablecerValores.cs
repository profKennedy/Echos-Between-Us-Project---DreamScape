using UnityEngine;
using UnityEngine.UI;

public class ResetValores : MonoBehaviour
{
    public Slider sliderVolumen;
    public Slider sliderBrillo;
    public float volumenDefault = 1f;
    public float brilloDefault = 0.5f;

    public void Restablecer()
    {
        if (sliderVolumen != null) sliderVolumen.value = volumenDefault;
        if (sliderBrillo != null) sliderBrillo.value = brilloDefault;
    }
}
