using Assets.Scripts.ScriptDani.Control_Central_Y_Estados;
using System;
using UnityEngine;

// Un reflector del escenario. Dani lo enciende con E. Va en el MISMO objeto que tiene el Collider
// (en la capa Interactuable), porque InteraccionPersonaje busca el IInteractable en el objeto del collider.
public class ReflectorEscenario : MonoBehaviour, IInteractable
{
    [Tooltip("Todo lo que se prende con el reflector: la Light, el cono de luz, un material emisivo... Arrancan apagados. NO pongas acá el objeto que tiene este script.")]
    [SerializeField] private GameObject[] objetosAlEncender;
    [SerializeField] private GameObject cartelUI;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxEncender;

    public bool EstaEncendido { get; private set; }

    // Lo escucha PuzzleReflectores para contar cuántos van
    public event Action<ReflectorEscenario> OnEncendido;

    private void Awake()
    {
        Alternar(false);
    }

    public void Interactuar(ControladorPersonaje jugador)
    {
        if (EstaEncendido) return;

        EstaEncendido = true;
        Alternar(true);
        MostrarPista(false);

        if (audioSource != null && sfxEncender != null) audioSource.PlayOneShot(sfxEncender);

        OnEncendido?.Invoke(this);
    }

    public void MostrarPista(bool mostrar)
    {
        if (cartelUI != null) cartelUI.SetActive(mostrar && !EstaEncendido);
    }

    private void Alternar(bool encendido)
    {
        foreach (GameObject objeto in objetosAlEncender)
        {
            if (objeto != null && objeto != gameObject) objeto.SetActive(encendido);
        }
    }
}