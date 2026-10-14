using UnityEngine;

public class EfectoEncogimiento : MonoBehaviour
{
    public float factorEncogimientoActual = 1f;
    public float factorObjetivo = 1f;
    public float factorMinimo = 0.2f;
    public Transform transformMesh;             // PIVOT padre del modelo, NO el objeto que tiene el Animator ni el root con collider
    public CapsuleCollider capsule;             // para ajustar altura físicamente
    public float velocidadTransicion = 2f;
    public float velocidadRecuperacion = 1f;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip sfxEncogerse;

    private float _alturaOriginal;
    private float _radioOriginal;
    private Vector3 _centroOriginal;
    private float _factorAplicadoAlCollider = 1f;
    private bool _yaSono = false;

    private NivelDeAutoestima _nivelAutoestimaDani;

    private void Awake()
    {
        if (capsule != null)
        {
            _alturaOriginal = capsule.height;
            _radioOriginal = capsule.radius;
            _centroOriginal = capsule.center;
        }

        if (transformMesh == null)
            Debug.LogWarning("EfectoEncogimiento: 'transformMesh' no está asignado en el Inspector, Dani no se va a encoger visualmente.", this);

        _nivelAutoestimaDani = GetComponent<NivelDeAutoestima>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (_nivelAutoestimaDani == null)
        {
            Debug.LogError("EfectoEncogimiento: NivelDeAutoestima debe estar en el MISMO GameObject.", this);
            return;
        }
        _nivelAutoestimaDani.OnAutoestimaCambiada += ActualizarTamaño;
    }

    private void OnDisable()
    {
        if (_nivelAutoestimaDani == null) return;
        _nivelAutoestimaDani.OnAutoestimaCambiada -= ActualizarTamaño;
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

        // Tocar el collider fuerza a PhysX a reconstruir la shape: solo si el factor cambió
        if (capsule != null && !Mathf.Approximately(factorEncogimientoActual, _factorAplicadoAlCollider))
        {
            capsule.height = _alturaOriginal * factorEncogimientoActual;
            capsule.radius = _radioOriginal * factorEncogimientoActual;
            capsule.center = _centroOriginal * factorEncogimientoActual;
            _factorAplicadoAlCollider = factorEncogimientoActual;
        }
    }

    // El Animator se evalúa DESPUÉS de Update y ANTES de LateUpdate.
    // Si algún clip tiene curvas de escala, pisa lo que hagamos en Update.
    private void LateUpdate()
    {
        if (transformMesh != null)
            transformMesh.localScale = Vector3.one * factorEncogimientoActual;
    }

    private void ActualizarTamaño(float porcentajeAutoestima)
    {
        factorObjetivo = Mathf.Lerp(factorMinimo, 1f, porcentajeAutoestima);
    }

    // OJO: este camino y ActualizarTamaño escriben factorObjetivo los dos.
    // Hoy nadie llama a AplicarPresion en lo que me pasaste. Decidí cuál es el camino oficial.
    public void AplicarPresion(float nivelNegatividad)
    {
        factorObjetivo = Mathf.Max(factorMinimo, 1f - nivelNegatividad);

        if (!_yaSono && audioSource != null && sfxEncogerse != null)
        {
            audioSource.PlayOneShot(sfxEncogerse);
            _yaSono = true;
        }
    }

    public void IniciarRecuperacion()
    {
        factorObjetivo = 1f;
        _yaSono = false;
    }

    public float ObtenerFactorActual() => factorEncogimientoActual;
    public bool EstaEncogida() => factorEncogimientoActual < 0.98f;
}