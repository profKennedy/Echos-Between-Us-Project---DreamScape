
using UnityEngine;

public class InterfazMobile : MonoBehaviour
{
    [SerializeField] private GameObject canvasControlesMobile;

    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        canvasControlesMobile.SetActive(true);
#else
        canvasControlesMobile.SetActive(false);
#endif
    }
}
