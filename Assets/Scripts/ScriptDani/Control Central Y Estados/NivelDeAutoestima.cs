using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NivelDeAutoestima : MonoBehaviour
{
    [Header("Valores")]
    public float autoestimaMaxima = 100f;
    public float autoestimaActual;

    [Header("Checkpoints")]
    public Transform ultimoCheckpoint;

    public event Action<float> OnAutoestimaCambiada;
    public event Action OnConsumidaPorNiebla;

    private ControladorPersonaje _controladorDani;
    private bool _estaDerrotada = false;

    private void Awake()
    {
        _controladorDani = GetComponent<ControladorPersonaje>();
        autoestimaActual = autoestimaMaxima;
    }

    private void Start()
    {
        OnAutoestimaCambiada?.Invoke(ObtenerPorcentaje());
    }

    public void ReducirAutoestima(float cantidad)
    {
        if (_estaDerrotada) return;

        autoestimaActual -= cantidad;
        if(autoestimaActual < 0f)
        {
            autoestimaActual = 0f;
            _estaDerrotada = true;
            ConsumidaPorNiebla();
        }
        OnAutoestimaCambiada?.Invoke(ObtenerPorcentaje());
    }

    public void RecuperarAutoestima(float cantidad)
    {
        if(_estaDerrotada)return;

        autoestimaActual += cantidad;
        if (autoestimaActual> autoestimaMaxima) 
        {
            autoestimaActual = autoestimaMaxima;
        }
        OnAutoestimaCambiada?.Invoke(ObtenerPorcentaje());
    }

    private void ConsumidaPorNiebla()
    {
        OnConsumidaPorNiebla?.Invoke();

        //Bloqueamos moviemiento mientras hacemos el respawn
        _controladorDani.movimiento.Bloquear(true);
        StartCoroutine(RutinaRespawn());
    }

    private IEnumerator RutinaRespawn()
    {
        yield return new WaitForSeconds(2f); //TiempoQeu dani queda derrotada

        //teletransportamos al ultimo checkpoint
        if(ultimoCheckpoint != null)
        {
            transform.position = ultimoCheckpoint.position;
            transform.rotation = ultimoCheckpoint.rotation;
        }
        else
        {
            Debug.LogWarning("no hay checkpoint asignado, se reinicia en la posicion actual");

        }
        autoestimaActual = autoestimaMaxima;
        _estaDerrotada = false;
        OnAutoestimaCambiada?.Invoke(ObtenerPorcentaje());

        _controladorDani.CambiarEstado(EstadoPersonaje.LIBRE);
        _controladorDani.movimiento.Bloquear(false);
    }

    public float ObtenerPorcentaje() => autoestimaActual/ autoestimaMaxima;

    public void SetCheckpoint(Transform nuevoCheckpoint)
    {
        ultimoCheckpoint = nuevoCheckpoint;
    }
}
