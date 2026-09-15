using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorPersonaje : MonoBehaviour
{
    public EstadoPersonaje estado;
    public MovimientoPersonaje movimiento;
    public InteraccionPersonaje interaccion;
    public EfectoEncogimiento efectoEncogimiento;
    public GestorEntradas entradas;
    public GestionLinterna gestionLinterna;

    public event Action OnPersonajeAsustado;
    public event Action<EstadoPersonaje> OnEstadoCambiado;

    [Space]
    public Camera camaraFreeLook;
    public Transform transformMano;

    private ModoFreeLook _modoFreeLook;
    private Modo2DLateral _modo2D;

    private bool _puedeAsustarse = true;
    private float _cooldownSusto = 2f;

    private void Awake()
    {
        _modoFreeLook = new ModoFreeLook(camaraFreeLook.transform);
        _modo2D = new Modo2DLateral();
    }

    private void Start()
    {
        movimiento.modoActual = _modoFreeLook;
        CambiarEstado(EstadoPersonaje.LIBRE);
    }

    private void OnEnable()
    {
        if (entradas == null || movimiento == null) return;

        entradas.AlMoverse += movimiento.Mover;
        entradas.AlSaltar += movimiento.Saltar;
        entradas.AlInteractuar += interaccion.IntentarInteractuar;
        entradas.AlAlternarLinterna += gestionLinterna.IntentarAlternarLuz;
        entradas.AlAlternarSigilo += OnSigiloInput;
    }

    private void OnDisable()
    {
        if (entradas == null || movimiento == null) return;

        entradas.AlMoverse -= movimiento.Mover;
        entradas.AlSaltar -= movimiento.Saltar;
        entradas.AlInteractuar -= interaccion.IntentarInteractuar;
        entradas.AlAlternarLinterna -= gestionLinterna.IntentarAlternarLuz;
        entradas.AlAlternarSigilo -= OnSigiloInput;
    }

    

    public void CambiarEstado(EstadoPersonaje nuevoEstado)
    {
        if (estado == nuevoEstado) return;
        estado = nuevoEstado;
        OnEstadoCambiado?.Invoke(estado);
    }

   

    private void OnSigiloInput(bool activar)
    {
        if (estado == EstadoPersonaje.ASUSTADA || estado == EstadoPersonaje.EN_FLASHBACK) return;

        if (activar)
        {
            CambiarEstado(EstadoPersonaje.SIGILO);
            movimiento.ActivarSigilo(true);
        }
        else
        {
            CambiarEstado(EstadoPersonaje.LIBRE);
            movimiento.ActivarSigilo(false);
        }
    }



    public void AlSerAsustada()
    {
        if (estado == EstadoPersonaje.EN_FLASHBACK) return;
        if (!_puedeAsustarse) return; // bloquea el spam

        _puedeAsustarse = false;
        CambiarEstado(EstadoPersonaje.ASUSTADA);
        movimiento.ActivarSigilo(false);
        gestionLinterna.SoltarLinternaPorSusto();
        OnPersonajeAsustado?.Invoke();

        StartCoroutine(RecuperarseDeSusto());
    }
    private IEnumerator RecuperarseDeSusto()
    {
        yield return new WaitForSeconds(_cooldownSusto);
        _puedeAsustarse = true;
        CambiarEstado(EstadoPersonaje.LIBRE); // vuelve a LIBRE después del susto
    }
    public void AlRecogerFragmento()
    {
        CambiarEstado(EstadoPersonaje.EN_FLASHBACK);
        movimiento.Bloquear(true);
        StartCoroutine(IrAEscenaFin());
    }

    private IEnumerator IrAEscenaFin()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("FinDelJuego");  
    }


    public void UsarCamaraFreeLook() => movimiento.modoActual = _modoFreeLook;
    public void UsarCamara2D() => movimiento.modoActual = _modo2D;
}