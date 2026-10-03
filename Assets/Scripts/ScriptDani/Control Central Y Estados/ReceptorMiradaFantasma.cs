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


    private NivelDeAutoestima _nivelAutoestimaDani;
    private void Awake()
    {
        _controlador = GetComponentInParent<ControladorPersonaje>();
        _nivelAutoestimaDani = GetComponentInParent<NivelDeAutoestima>();
    }

    private void Update()
    {
        if (_siendoMirada)
        {
            _nivelAutoestimaDani.ReducirAutoestima(15f*Time.deltaTime);
        }
        else
        {
            _nivelAutoestimaDani.RecuperarAutoestima(5f*Time.deltaTime);
        }

        _siendoMirada = false; 
    }

    // Los fantasmas llaman esto con su Raycast
    public void RecibirMirada()
    {
        _siendoMirada = true;
    }
}
