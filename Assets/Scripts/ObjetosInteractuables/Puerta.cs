using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System.Collections;
using UnityEngine;

public class Puerta : MonoBehaviour, IInteractable
{
    [Header("Cerradura")]
    [Tooltip("Llave que abre esta puerta. Vacío = no necesita llave.")]
    [SerializeField] private DatosItem llaveRequerida;

    [Tooltip("Marcalo si la puerta NO se abre con E sino por un mecanismo (reflectores, palanca...). Se abre desde código con AbrirPorMecanismo().")]
    [SerializeField] private bool bloqueadaPorMecanismo;

    [Header("Apertura")]
    [SerializeField] private float alturaApertura = 5f;
    [SerializeField] private float velocidadApertura = 2f;

    [Header("UI")]
    [SerializeField] private GameObject cartelUI;

    private bool _abierta;

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (_abierta) return;

        if (bloqueadaPorMecanismo)
        {
            Debug.Log("La puerta no cede: algo más la mantiene cerrada.");
            return;
        }

        if (llaveRequerida != null)
        {
            if (jugador.mochila == null || !jugador.mochila.TryUsar(llaveRequerida))
            {
                Debug.Log($"La puerta está cerrada. Necesitás: {llaveRequerida.nombre}");
                return;
            }
        }

        Abrir();
    }

    // La llama el mecanismo que la controla (PuzzleReflectores)
    public void AbrirPorMecanismo()
    {
        if (_abierta) return;

        bloqueadaPorMecanismo = false;
        Abrir();
    }

    public void MostrarPista(bool mostrar)
    {
        // Si la abre un mecanismo, el cartel "E - Abrir" sería mentira
        if (cartelUI != null) cartelUI.SetActive(mostrar && !_abierta && !bloqueadaPorMecanismo);
    }

    private void Abrir()
    {
        _abierta = true;
        MostrarPista(false);
        StartCoroutine(RutinaApertura());
    }

    private IEnumerator RutinaApertura()
    {
        Vector3 destino = transform.position + Vector3.up * alturaApertura;

        while (transform.position != destino)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidadApertura * Time.deltaTime);
            yield return null;
        }
    }
}