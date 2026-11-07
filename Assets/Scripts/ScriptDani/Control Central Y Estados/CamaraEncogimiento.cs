
using UnityEngine;
using Cinemachine;

public class CamaraEncogimiento : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CinemachineFreeLook camaraDani;
    [SerializeField] private EfectoEncogimiento efectoEncogimiento;

    [Header("Configuracion")]
    [SerializeField, Range(0.1f, 1f)]
    private float factorCamaraMinimo = 0.25f;

    [SerializeField]
    private float velocidadSuavizado = 8f;

    private readonly float[] alturasOriginales = new float[3];
    private readonly float[] radiosOriginales = new float[3];

    private void Start()
    {
        if (camaraDani == null || efectoEncogimiento == null)
        {
            Debug.LogError(
                "CamaraEncogimiento: faltan referencias.",
                this
            );

            enabled = false;
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            alturasOriginales[i] =
                camaraDani.m_Orbits[i].m_Height;

            radiosOriginales[i] =
                camaraDani.m_Orbits[i].m_Radius;
        }
    }

    private void LateUpdate()
    {
        float factorDani =
            efectoEncogimiento.ObtenerFactorActual();

        float progreso = Mathf.InverseLerp(
            efectoEncogimiento.factorMinimo,
            1f,
            factorDani
        );

        float factorCamara = Mathf.Lerp(
            factorCamaraMinimo,
            1f,
            progreso
        );

        // Suavizado independiente de los FPS.
        float suavizado = 1f - Mathf.Exp(
            -velocidadSuavizado * Time.deltaTime
        );

        for (int i = 0; i < 3; i++)
        {
            var orbit = camaraDani.m_Orbits[i];

            orbit.m_Height = Mathf.Lerp(
                orbit.m_Height,
                alturasOriginales[i] * factorCamara,
                suavizado
            );

            orbit.m_Radius = Mathf.Lerp(
                orbit.m_Radius,
                radiosOriginales[i] * factorCamara,
                suavizado
            );

            camaraDani.m_Orbits[i] = orbit;
        }
    }
}
