using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceptorMiradaFantasma : MonoBehaviour
{
    [Header("Configuración")]
    public float nivelNegatividadPorSegundo = 0.3f;  // qué tan rápido se encoge
    public float umbralParaAsustar = 0.5f;            // si el factor baja de esto, se asusta

    private ControladorPersonaje _controlador;
    private EfectoEncogimiento _encogimiento;
    private bool _siendoMirada = false;

    private void Awake()
    {
        _controlador = GetComponentInParent<ControladorPersonaje>();
        _encogimiento = GetComponentInParent<EfectoEncogimiento>();
    }

    private void Update()
    {
        if (_siendoMirada)
        {
            _encogimiento.AplicarPresion(
                _encogimiento.ObtenerFactorActual() - nivelNegatividadPorSegundo * Time.deltaTime
                    < 0 ? 1f : 1f - (1f - _encogimiento.ObtenerFactorActual() + nivelNegatividadPorSegundo * Time.deltaTime)
            );

            // Si encogió demasiado, asustar
            if (_encogimiento.ObtenerFactorActual() <= umbralParaAsustar)
            {
                _controlador.AlSerAsustada();
            }
        }
        else
        {
            _encogimiento.IniciarRecuperacion();
        }

        _siendoMirada = false; 
    }

    // Los fantasmas llaman esto con su Raycast
    public void RecibirMirada()
    {
        _siendoMirada = true;
    }
}
