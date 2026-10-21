
using System;
using UnityEngine;

public class GestorEntradas : MonoBehaviour
{
    public Vector2 entradaMovimiento;
    public bool saltarPresionado;
    public bool interactuarPresionado;
    public bool alternarLinternaPresionado;
    public bool soltarLinternaPresionado;

    public event Action<Vector2> AlMoverse;
    public event Action<bool> AlAlternarSigilo;
    public event Action AlSaltar, AlInteractuar,
        AlAlternarLinterna, AlSoltarLinterna;
    private bool _sigiloMobileActivo = false;


    // Movimiento recibido desde el joystick virtual.
    private Vector2 _movimientoMobile;

    // Indica si se están usando controles móviles.
    [SerializeField] private bool usarControlesMobile = false;

    private void Update()
    {
        // Elegimos de dónde obtener el movimiento.
        if (usarControlesMobile)
        {
            entradaMovimiento = _movimientoMobile;
        }
        else
        {
            entradaMovimiento = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
            );
        }

        // Enviamos el movimiento al personaje.
        AlMoverse?.Invoke(entradaMovimiento);

        // Entradas del teclado para PC.
        if (!usarControlesMobile)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                AlAlternarSigilo?.Invoke(true);

            if (Input.GetKeyUp(KeyCode.LeftShift))
                AlAlternarSigilo?.Invoke(false);

            saltarPresionado = Input.GetButtonDown("Jump");
            if (saltarPresionado)
                AlSaltar?.Invoke();

            interactuarPresionado = Input.GetKeyDown(KeyCode.E);
            if (interactuarPresionado)
                AlInteractuar?.Invoke();

            alternarLinternaPresionado = Input.GetKeyDown(KeyCode.F);
            if (alternarLinternaPresionado)
                AlAlternarLinterna?.Invoke();
        }
    }

    // MÉTODOS PARA CONTROLES MÓVILES

    public void EstablecerMovimientoMobile(Vector2 direccion)
    {
        _movimientoMobile = Vector2.ClampMagnitude(direccion, 1f);
    }

    public void SaltarMobile()
    {
        AlSaltar?.Invoke();
    }

    public void InteractuarMobile()
    {
        AlInteractuar?.Invoke();
    }

    public void AlternarLinternaMobile()
    {
        AlAlternarLinterna?.Invoke();
    }

    public void AlternarSigiloMobile()
    {
        _sigiloMobileActivo = !_sigiloMobileActivo;
        AlAlternarSigilo?.Invoke(_sigiloMobileActivo);
    }

    public void DesactivarSigiloMobile()
    {
        AlAlternarSigilo?.Invoke(false);
    }

    private void Awake()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        usarControlesMobile = true;
    #else
            usarControlesMobile = false;
    #endif
    }
}
