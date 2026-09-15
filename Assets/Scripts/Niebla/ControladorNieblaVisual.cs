using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Conecta los datos de GestorNiebla (nivel de negatividad por zona)
/// con la representacion visual de la niebla de piso: el material que usa
/// NieblaSuelo.shader y el Fog global de URP (RenderSettings).
///
/// Requiere que RenderSettings.fog este activado y en modo Linear
/// (Lighting > Environment > Fog) para que el ajuste de distancia tenga efecto.
/// Si se prefiere modo Exponential, ajustar RenderSettings.fogDensity en su lugar.
/// </summary>
public class ControladorNieblaVisual : MonoBehaviour
{
    [SerializeField] private GestorNiebla gestorNiebla;
    [SerializeField] private Renderer[] planosNieblaSuelo;
    [SerializeField] private Transform jugador;
    [SerializeField] private ControladorLinterna linterna;

    [Header("Curva de densidad del plano de niebla")]
    [SerializeField] private float densidadMinima = 0.15f;
    [SerializeField] private float densidadMaxima = 0.75f;

    [Header("Curva de distancia de Fog URP (modo Linear)")]
    [SerializeField] private float distanciaFogPocaNiebla = 35f;
    [SerializeField] private float distanciaFogMuchaNiebla = 8f;

    [Header("Reaccion a la linterna")]
    [SerializeField] private bool linternaDisipaNiebla = true;
    [SerializeField] private float factorDisipacionLinterna = 0.5f;

    private static readonly int DensidadID = Shader.PropertyToID("_Densidad");

    private void Update()
    {
        if (gestorNiebla == null || jugador == null) return;

        float nivel = gestorNiebla.ObtenerNivelEnPosicion(jugador.position);

        //if (linternaDisipaNiebla && linterna != null && linterna.EstaEncendida)
        //{
          //  nivel *= factorDisipacionLinterna;
        //}

        float densidadActual = Mathf.Lerp(densidadMinima, densidadMaxima, nivel);
        ActualizarPlanosDeNiebla(densidadActual);

        RenderSettings.fogEndDistance = Mathf.Lerp(distanciaFogPocaNiebla, distanciaFogMuchaNiebla, nivel);
    }

    private void ActualizarPlanosDeNiebla(float densidad)
    {
        if (planosNieblaSuelo == null) return;

        foreach (var plano in planosNieblaSuelo)
        {
            if (plano == null) continue;
            plano.material.SetFloat(DensidadID, densidad);
        }
    }
}