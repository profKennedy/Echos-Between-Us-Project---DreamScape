using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System.Collections;
using UnityEngine;

public class Puerta : MonoBehaviour, IInteractable
{
    [Header("Cerradura")]
    [Tooltip("Llave que abre esta puerta. Vacío = no necesita llave.")]
    [SerializeField] private DatosItem llaveRequerida;

    [Header("Apertura")]
    [SerializeField] private float alturaApertura = 5f;
    [SerializeField] private float velocidadApertura = 2f;

    [Header("UI")]
    [SerializeField] private GameObject cartelUI;

    private bool _abierta;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (_abierta) return;

        if (llaveRequerida != null)
        {
            if (jugador.mochila == null || !jugador.mochila.TryUsar(llaveRequerida))
            {
                Debug.Log($"La puerta está cerrada. Necesitás: {llaveRequerida.nombre}");
                return;
            }
        }

        _abierta = true;
        MostrarPista(false);
        StartCoroutine(Abrir());
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelUI != null) cartelUI.SetActive(mostrar && !_abierta);
    }

    private IEnumerator Abrir()
    {
        Vector3 destino = transform.position + Vector3.up * alturaApertura;

        while (transform.position != destino)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidadApertura * Time.deltaTime);
            yield return null;
        }
    }
}