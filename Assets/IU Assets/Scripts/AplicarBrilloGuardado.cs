// AplicarBrilloGuardado.cs  (va en BrilloPantalla)
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AplicarBrilloGuardado : MonoBehaviour
{
    private void Awake()
    {
        Image panel = GetComponent<Image>();
        Color c = panel.color;
        c.a = PlayerPrefs.GetFloat(BrilloController.ClaveBrillo, BrilloController.BrilloPorDefecto);
        panel.color = c;
        panel.raycastTarget = false;
    }
}