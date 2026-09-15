using UnityEngine;

public class DetectorLuzAgresor : MonoBehaviour
{
    [SerializeField] private Agresor agresor;
    [SerializeField] private ControladorLinterna linterna;
    [SerializeField] private LayerMask capasAIgnorar;

    [Tooltip("Cada cuántos segundos verifica si la luz lo está iluminando")]
    [SerializeField] private float intervaloChequeo = 0.2f;

    private float timerChequeo = 0f;

    private void Update()
    {
        if (linterna == null || agresor == null) return;

        if (!linterna.EstaEncendida()) return;

        timerChequeo += Time.deltaTime;
        if (timerChequeo >= intervaloChequeo)
        {
            timerChequeo = 0f;
            VerificarSiLuzIluminaAgresor();
        }
    }

    private void VerificarSiLuzIluminaAgresor()
    {
        Light luz = linterna.componenteLuz;
        if (luz == null) return;

        Vector3 posLuz = luz.transform.position;
        Vector3 dirAgresor = (transform.position - posLuz).normalized;
        float distancia = Vector3.Distance(posLuz, transform.position);

        if (distancia > agresor.umbralLuzDeteccion) return;

        float angulo = Vector3.Angle(luz.transform.forward, dirAgresor);

        if (angulo > luz.spotAngle / 2f) return;

        RaycastHit hit;
        if (Physics.Raycast(posLuz, dirAgresor, out hit, distancia, ~capasAIgnorar)) return;

        agresor.DetectarLuz(linterna.intensidadLuz, posLuz);
    }
}