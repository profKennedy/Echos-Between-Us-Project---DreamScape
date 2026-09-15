using UnityEngine;
using System.Collections.Generic;

public class GestorNiebla : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ControladorLinterna linterna;

    [Header("Configuración")]
    [SerializeField] private float nivelNegatividadGlobal = 0.1f;
    [SerializeField] private float intervaloChequeo = 0.2f;
    [SerializeField] private float distanciaMaximaLinterna = 15f;

    private List<ZonaNegatividad> zonasActivas = new List<ZonaNegatividad>();
    private float timer = 0f;

    private void Update()
    {

        if (linterna == null || !linterna.EstaEncendida())
        {
            foreach (var zona in zonasActivas)
                zona.DisolverlPorLinterna(false);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= intervaloChequeo)
        {
            timer = 0f;
            VerificarLinternaSobreZonas();
        }
    }

    private void VerificarLinternaSobreZonas()
    {
        Light luz = linterna.componenteLuz;
        if (luz == null) { Debug.Log("componenteLuz es null"); return; }

        foreach (var zona in zonasActivas)
        {
            Vector3 posLuz = luz.transform.position;
            Vector3 dirAZona = (zona.transform.position - posLuz).normalized;
            float distancia = Vector3.Distance(posLuz, zona.transform.position);
            float angulo = Vector3.Angle(luz.transform.forward, dirAZona);

            bool iluminada = angulo < luz.spotAngle / 2f && distancia < distanciaMaximaLinterna;

            

            zona.DisolverlPorLinterna(iluminada);
        }
    }

    public void RegistrarZona(ZonaNegatividad zona)
    {
        if (!zonasActivas.Contains(zona))
            zonasActivas.Add(zona);
    }

    public float ObtenerNivelEnPosicion(Vector3 pos)
    {
        float nivelMaximo = nivelNegatividadGlobal;
        foreach (var zona in zonasActivas)
        {
            if (zona.EstaEnZona(pos))
            {
                float nivelZona = zona.ObtenerNivelNegatividad();
                if (nivelZona > nivelMaximo) nivelMaximo = nivelZona;
            }
        }
        return nivelMaximo;
    }
}