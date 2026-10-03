using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarraDeInfluenciaNegativa : MonoBehaviour
{
    [SerializeField] private Slider BarraAutoestimaSlider;
    [SerializeField] private NivelDeAutoestima autoestimaDani;

    private void OnEnable()
    {
        if(autoestimaDani != null)
        {
            autoestimaDani.OnAutoestimaCambiada += ActualizarBarra;
        }
    }

    private void OnDisable()
    {
        if(autoestimaDani != null)
        {
            autoestimaDani.OnAutoestimaCambiada -= ActualizarBarra;
        }
    }
    
    private void ActualizarBarra(float porcentaje)
    {
        BarraAutoestimaSlider.value = porcentaje;
    }
}
