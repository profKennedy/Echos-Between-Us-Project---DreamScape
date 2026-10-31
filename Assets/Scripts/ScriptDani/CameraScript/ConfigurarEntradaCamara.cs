using UnityEngine;
using Cinemachine;

public class ConfigurarEntradaCamara : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook camaraDani;

    private void Awake()
    {
        if (camaraDani == null)
        {
            Debug.LogError("No se asigno CamaraDani.");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR

        // Android: Cinemachine no lee el mouse.
        // El joystick movil controla la camara por separado.
        camaraDani.m_XAxis.m_InputAxisName = "";
        camaraDani.m_YAxis.m_InputAxisName = "";

#else

        // PC: Cinemachine utiliza el mouse.
        camaraDani.m_XAxis.m_InputAxisName = "Mouse X";
        camaraDani.m_YAxis.m_InputAxisName = "Mouse Y";

#endif
    }
}