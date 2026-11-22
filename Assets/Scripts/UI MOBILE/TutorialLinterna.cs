
using System.Collections;
using UnityEngine;

public class TutorialLinterna : MonoBehaviour
{
    [Header("Interfaz")]
    [SerializeField] private GameObject panelTutorial;

    [Header("Referencias")]
    [SerializeField] private ControladorLinterna controladorLinterna;

    [Header("Configuración")]
    [SerializeField] private float esperaAntesDeMostrar = 2f;

    private bool tutorialMostrado = false;
    private bool esperandoEncendido = false;
    private Coroutine rutinaTutorial;

    private void Awake()
    {
        if (panelTutorial != null)
            panelTutorial.SetActive(false);
    }

    private void OnEnable()
    {
        if (controladorLinterna != null)
            controladorLinterna.OnLuzCambiada += AlCambiarLuz;
    }

    private void OnDisable()
    {
        if (controladorLinterna != null)
            controladorLinterna.OnLuzCambiada -= AlCambiarLuz;

        if (rutinaTutorial != null)
        {
            StopCoroutine(rutinaTutorial);
            rutinaTutorial = null;
        }
    }

    public void MostrarTutorial()
    {
        if (tutorialMostrado)
            return;

        tutorialMostrado = true;
        esperandoEncendido = true;

        rutinaTutorial = StartCoroutine(EsperarYMostrar());
    }

    private IEnumerator EsperarYMostrar()
    {
        yield return new WaitForSeconds(esperaAntesDeMostrar);

        if (esperandoEncendido && panelTutorial != null)
            panelTutorial.SetActive(true);

        rutinaTutorial = null;
    }

    private void AlCambiarLuz(bool encendida)
    {
        if (!esperandoEncendido || !encendida)
            return;

        esperandoEncendido = false;

        if (rutinaTutorial != null)
        {
            StopCoroutine(rutinaTutorial);
            rutinaTutorial = null;
        }

        if (panelTutorial != null)
            panelTutorial.SetActive(false);
    }
}
