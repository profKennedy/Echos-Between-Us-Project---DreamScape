using UnityEngine;

// Volumen invisible debajo de los pozos (o de cualquier lugar donde Dani no deba caer).
// Si Dani lo toca, queda derrotada y reaparece en el último checkpoint.
[RequireComponent(typeof(Collider))]
public class ZonaCaida : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        NivelDeAutoestima autoestima = other.GetComponentInParent<NivelDeAutoestima>();
        if (autoestima != null) autoestima.Derrotar();
    }
}