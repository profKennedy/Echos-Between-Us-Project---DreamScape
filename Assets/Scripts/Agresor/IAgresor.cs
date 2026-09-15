using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAgresor
{
    bool DetectarLuz(float intensidad, Vector3 posicion);

    bool DetectarJugador(Vector3 posJugador);

    void CambiarEstadoAgresor(EstadoAgresor nuevoEstado);
}