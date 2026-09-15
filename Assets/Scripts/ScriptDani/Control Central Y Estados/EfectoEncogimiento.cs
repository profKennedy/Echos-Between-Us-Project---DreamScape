using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EfectoEncogimiento : MonoBehaviour
{
    public float factorEncogimientoActual = 1f;
    public float factorObjetivo = 1f;
    public float factorMinimo = 0.2f;
    public Transform transformMesh;             // solo el mesh, NO el root con collider
    public CapsuleCollider capsule;             // para ajustar altura físicamente
    public float velocidadTransicion = 2f;
    public float velocidadRecuperacion = 1f;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip sfxEncogerse;

    private float _alturaOriginal;
    private float _radioOriginal;

    private bool _yaSono = false;

    private void Awake()
    {
        if (capsule != null)
        {
            _alturaOriginal = capsule.height;
            _radioOriginal = capsule.radius;
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        // Interpolar suavemente hacia el objetivo
        float velocidad = factorEncogimientoActual > factorObjetivo
            ? velocidadTransicion
            : velocidadRecuperacion;

        factorEncogimientoActual = Mathf.MoveTowards(
            factorEncogimientoActual,
            factorObjetivo,
            velocidad * Time.deltaTime
        );

        // Solo escalar el mesh visual
        if (transformMesh != null)
            transformMesh.localScale = Vector3.one * factorEncogimientoActual;

        // Ajustar el collider proporcionalmente
        if (capsule != null)
        {
            capsule.height = _alturaOriginal * factorEncogimientoActual;
            capsule.radius = _radioOriginal * factorEncogimientoActual;
        }
    }
    // Llamado por ReceptorMiradaFantasma cada frame que la miran
    public void AplicarPresion(float nivelNegatividad)
    {
        factorObjetivo = Mathf.Max(factorMinimo, 1f - nivelNegatividad);

        if (!_yaSono && audioSource != null && sfxEncogerse != null)
        {
            audioSource.PlayOneShot(sfxEncogerse);
            _yaSono = true; // Lo marcamos como reproducido
        }
    }

    // Llamado cuando dejan de mirarla
    public void IniciarRecuperacion()
    {
        factorObjetivo = 1f;
        _yaSono = false;
    }
    public float ObtenerFactorActual() => factorEncogimientoActual;
    public bool EstaEncogida() => factorEncogimientoActual < 0.98f;
}
