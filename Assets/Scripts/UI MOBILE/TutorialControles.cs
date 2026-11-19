
using UnityEngine;

public class TutorialControles : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject tutorialPC;
    [SerializeField] private GameObject tutorialMobile;

    [Header("Configuración")]
    [SerializeField] private float duracionMinima = 5f;

    private float tiempoInicio;
    private bool tutorialActivo;

    private void Awake()
    {
        tutorialPC.SetActive(false);
        tutorialMobile.SetActive(false);
    }

    private void Update()
    {
        if (!tutorialActivo)
            return;

        if (Time.time - tiempoInicio < duracionMinima)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        // En Android, cerrar al detectar
        // un deslizamiento del lado derecho.
        if (Input.touchCount > 0)
        {
            foreach (Touch toque in Input.touches)
            {
                if (toque.position.x > Screen.width * 0.5f &&
                    toque.phase == TouchPhase.Moved)
                {
                    OcultarTutorial();
                    break;
                }
            }
        }
#else
        // En PC, cerrar al comenzar a moverse.
        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f)
        {
            OcultarTutorial();
        }
#endif
    }

    public void MostrarTutorial()
    {
        tutorialActivo = true;
        tiempoInicio = Time.time;

#if UNITY_ANDROID && !UNITY_EDITOR
        tutorialMobile.SetActive(true);
        tutorialPC.SetActive(false);
#else
        tutorialPC.SetActive(true);
        tutorialMobile.SetActive(false);
#endif
    }

    public void OcultarTutorial()
    {
        tutorialActivo = false;
        tutorialPC.SetActive(false);
        tutorialMobile.SetActive(false);
    }
}
