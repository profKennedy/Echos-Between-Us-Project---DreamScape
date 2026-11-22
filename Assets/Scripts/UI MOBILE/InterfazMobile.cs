using UnityEngine;

public class InterfazMobile : MonoBehaviour
{
    [SerializeField] private GameObject canvasControlesMobile;

    private void Awake()
    {
        MostrarControles(true);
    }

    public void MostrarControles(bool mostrar)
    {
        if (canvasControlesMobile == null)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        canvasControlesMobile.SetActive(mostrar);
#else
        canvasControlesMobile.SetActive(false);
#endif
    }
}