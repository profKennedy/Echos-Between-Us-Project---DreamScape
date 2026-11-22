
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBateriaLinterna : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ControladorLinterna controladorLinterna;
    [SerializeField] private GestionLinterna gestionLinterna;

    [Header("Interfaz")]
    [SerializeField] private GameObject panelBateria;
    [SerializeField] private Image rellenoBateria;
    [SerializeField] private TMP_Text textoPorcentaje;

    [Header("Colores")]
    [SerializeField] private Color colorNormal = new Color(0.85f, 0.75f, 0.45f);
    [SerializeField] private Color colorBajo = new Color(0.9f, 0.3f, 0.3f);

    private void OnEnable()
    {
        if (controladorLinterna != null)
            controladorLinterna.OnBateriaCambiada += ActualizarBateria;

        ActualizarInterfaz();
    }

    private void OnDisable()
    {
        if (controladorLinterna != null)
            controladorLinterna.OnBateriaCambiada -= ActualizarBateria;
    }

    private void Update()
    {
        if (gestionLinterna == null || panelBateria == null)
            return;

        bool mostrar = gestionLinterna.EstaEnMano;

        if (panelBateria.activeSelf != mostrar)
            panelBateria.SetActive(mostrar);
    }

    private void ActualizarInterfaz()
    {
        if (controladorLinterna == null)
            return;

        ActualizarBateria(controladorLinterna.ObtenerPorcentajeBateria());
    }

    private void ActualizarBateria(float porcentaje)
    {
        porcentaje = Mathf.Clamp01(porcentaje);

        if (rellenoBateria != null)
        {
            rellenoBateria.fillAmount = porcentaje;
            rellenoBateria.color =
                porcentaje <= 0.20f ? colorBajo : colorNormal;
        }

        if (textoPorcentaje != null)
            textoPorcentaje.text =
                Mathf.RoundToInt(porcentaje * 100f) + "%";
    }
}
