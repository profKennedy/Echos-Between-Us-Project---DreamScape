using System;
using System.Collections;
using System.Collections.Generic;
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
    public event Action AlSaltar, AlInteractuar, AlAlternarLinterna, AlSoltarLinterna;

    private void Update()
    {
        entradaMovimiento = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        AlMoverse?.Invoke(entradaMovimiento);
        if (entradaMovimiento != Vector2.zero) AlMoverse?.Invoke(entradaMovimiento);

        if (Input.GetKeyDown(KeyCode.LeftShift)) AlAlternarSigilo?.Invoke(true);
        if (Input.GetKeyUp(KeyCode.LeftShift)) AlAlternarSigilo?.Invoke(false);

        saltarPresionado = Input.GetButtonDown("Jump");
        if (saltarPresionado) AlSaltar?.Invoke();

        interactuarPresionado = Input.GetKeyDown(KeyCode.E);
        if (interactuarPresionado) AlInteractuar?.Invoke();

        alternarLinternaPresionado = Input.GetKeyDown(KeyCode.F);
        if (alternarLinternaPresionado) AlAlternarLinterna?.Invoke();
    }
}
