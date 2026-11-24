// BrilloController.cs
using UnityEngine;
using UnityEngine.UI;

public class BrilloController : MonoBehaviour
{
    public const string ClaveBrillo = "brillo";
    public const float BrilloPorDefecto = 0.5f;

    [SerializeField] private Slider slider;
    [SerializeField] private Image panelBrillo;

    private void Start()
    {
        if (slider == null || panelBrillo == null)
        {
            Debug.LogError("BrilloController: falta asignar 'Slider' o 'Panel Brillo'.", this);
            return;
        }

        float guardado = PlayerPrefs.GetFloat(ClaveBrillo, BrilloPorDefecto);
        slider.SetValueWithoutNotify(guardado);
        Aplicar(guardado);
    }

    public void ChangeSlider(float valor)
    {
        PlayerPrefs.SetFloat(ClaveBrillo, valor);
        Aplicar(valor);
    }

    private void Aplicar(float valor)
    {
        if (panelBrillo == null) return;
        Color c = panelBrillo.color;
        c.a = valor;
        panelBrillo.color = c;
    }
}