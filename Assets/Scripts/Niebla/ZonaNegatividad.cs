using UnityEngine;

public class ZonaNegatividad : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float nivelNegatividad = 0.5f;
    [SerializeField] private float radio = 10f;

    [Header("Partículas")]
    [SerializeField] private ParticleSystem particulasNiebla;

    [Header("Efecto Linterna")]
    [SerializeField] private float velocidadDisolucion = 2f;
    [SerializeField] private float emisionNormal = 50f;
    [SerializeField] private float emisionDisuelta = 0f;
    [SerializeField] private float tamañoNormal = 2.5f;
    [SerializeField] private float tamañoDisuelto = 0.3f;

    private bool linternaApuntando = false;
    private float emisionActual;
    private float tamañoActual;

    private void Start()
    {
        FindObjectOfType<GestorNiebla>()?.RegistrarZona(this);
        emisionActual = emisionNormal;
        tamañoActual = tamañoNormal;

        if (particulasNiebla != null)
            particulasNiebla.Play();
    }

    private void Update()
    {
        if (particulasNiebla == null) return;

        float emisionObjetivo = linternaApuntando ? emisionDisuelta : emisionNormal;
        float tamañoObjetivo = linternaApuntando ? tamañoDisuelto : tamañoNormal;

        emisionActual = Mathf.Lerp(emisionActual, emisionObjetivo, Time.deltaTime * velocidadDisolucion);
        tamañoActual = Mathf.Lerp(tamañoActual, tamañoObjetivo, Time.deltaTime * velocidadDisolucion);

        // Forzar stop/play según estado
        if (linternaApuntando && particulasNiebla.isPlaying && emisionActual < 1f)
        {
            particulasNiebla.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Debug.Log("Partículas detenidas");
        }
        else if (!linternaApuntando && !particulasNiebla.isPlaying)
        {
            particulasNiebla.Play();
            Debug.Log("Partículas reiniciadas");
        }

        var main = particulasNiebla.main;
        main.startSize = tamañoActual;
    }

    public void DisolverlPorLinterna(bool disolviendo)
    {
        linternaApuntando = disolviendo;
    }

    public bool EstaEnZona(Vector3 pos)
    {
        return Vector3.Distance(transform.position, pos) <= radio;
    }

    public float ObtenerNivelNegatividad() => nivelNegatividad;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f);
        Gizmos.DrawSphere(transform.position, radio);
    }
}