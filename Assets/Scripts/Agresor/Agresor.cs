using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Agresor : MonoBehaviour, IAgresor
{
    // -------------------------------------------------------
    // CONFIGURACIÓN
    // -------------------------------------------------------
    [Header("Detección")]
    [SerializeField] private float radioDeteccion = 10f;
    [SerializeField] private float anguloVision = 90f;
    [SerializeField] public float umbralLuzDeteccion = 15f;
    [SerializeField] private LayerMask capasObstaculo;

    [Header("Patrulla")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float tiempoEsperaWaypoint = 1.5f;

    [Header("Comportamiento")]
    [SerializeField] private float velocidadPatrulla = 2.5f;
    [SerializeField] private float velocidadPersecucion = 5f;
    [SerializeField] private float tiempoMaximoAlerta = 4f;
    [SerializeField] private float distanciaAtaque = 1.2f;

    // -------------------------------------------------------
    // ESTADO INTERNO
    // -------------------------------------------------------
    private NavMeshAgent agente;
    private EstadoAgresor estadoActual;
    private Transform objetivoJugador;
    private Vector3 ultimaPosicionSospechosa;
    private int waypointActual = 0;
    private bool esperandoEnWaypoint = false;
    private float timerAlerta = 0f;
    private bool repelido = false;
    private float timerRepelido = 0f;
    [SerializeField] private float duracionRepelido = 3f;

    // -------------------------------------------------------
    // EVENTOS
    // -------------------------------------------------------
    public event Action OnJugadorDetectado;
    public event Action OnAgresorAlerta;
    public event Action OnAgresorVuelveAPatrullar;

    // -------------------------------------------------------
    // UNITY
    // -------------------------------------------------------
    private void Awake()
    {
        agente = GetComponent<NavMeshAgent>();

        ControladorPersonaje jugador = FindFirstObjectByType<ControladorPersonaje>();
        if (jugador != null)
            objetivoJugador = jugador.transform;
        else
            Debug.LogWarning("Agresor: No se encontró ControladorPersonaje en la escena.");

        CambiarEstadoAgresor(EstadoAgresor.PATRULLANDO);
    }

    private void Update()
    {
        if (objetivoJugador == null) return;
        ActualizarComportamiento();
    }

    // -------------------------------------------------------
    // MÁQUINA DE ESTADOS
    // -------------------------------------------------------
    private void ActualizarComportamiento()
    {
        // Si está repelido, cuenta el timer y bloquea la detección visual
        if (repelido)
        {
            timerRepelido += Time.deltaTime;
            if (timerRepelido >= duracionRepelido)
            {
                repelido = false;
                timerRepelido = 0f;
            }
        }

        switch (estadoActual)
        {
            case EstadoAgresor.PATRULLANDO:
                Patrullar();
                if (!repelido) VerificarVisionJugador(); // <- no detecta mientras huye
                break;

            case EstadoAgresor.ALERTA:
                timerAlerta += Time.deltaTime;
                MoverHacia(ultimaPosicionSospechosa);
                if (!repelido) VerificarVisionJugador();
                if (timerAlerta >= tiempoMaximoAlerta)
                    CambiarEstadoAgresor(EstadoAgresor.PATRULLANDO);
                break;

            case EstadoAgresor.PERSIGUIENDO:
                Perseguir();
                if (!JugadorEnRango())
                    CambiarEstadoAgresor(EstadoAgresor.ALERTA);
                break;
        }
    }

    // -------------------------------------------------------
    // COMPORTAMIENTOS
    // -------------------------------------------------------
    private void Patrullar()
    {
        if (waypoints.Length == 0) return;
        if (esperandoEnWaypoint) return;

        if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance)
        {
            StartCoroutine(EsperarYAvanzarWaypoint());
        }
    }

    private IEnumerator EsperarYAvanzarWaypoint()
    {
        esperandoEnWaypoint = true;
        yield return new WaitForSeconds(tiempoEsperaWaypoint);
        waypointActual = (waypointActual + 1) % waypoints.Length;
        agente.SetDestination(waypoints[waypointActual].position);
        esperandoEnWaypoint = false;
    }

    private void Perseguir()
    {
        agente.SetDestination(objetivoJugador.position);

        if (Vector3.Distance(transform.position, objetivoJugador.position) <= distanciaAtaque)
        {
            //llamar a jugador.AlSerAsustada()
            // Obtener el componente ControladorPersonaje del GameObject del jugador
            ControladorPersonaje jugador = objetivoJugador.GetComponent<ControladorPersonaje>();

            if (jugador != null)
            {
                jugador.AlSerAsustada();
                Debug.Log("¡Agresor alcanzó al jugador!");
            }
            else
            {
                Debug.LogWarning("No se encontró ControladorPersonaje en el jugador");
            }
        }
    }

    private void MoverHacia(Vector3 destino)
    {
        agente.SetDestination(destino);
    }

    // -------------------------------------------------------
    // DETECCIÓN
    // -------------------------------------------------------
    private void VerificarVisionJugador()
    {
        if (DetectarJugador(objetivoJugador.position))
        {
            ultimaPosicionSospechosa = objetivoJugador.position;
            CambiarEstadoAgresor(EstadoAgresor.PERSIGUIENDO);
        }
    }

    public bool DetectarJugador(Vector3 posJugador)
    {
        float distancia = Vector3.Distance(transform.position, posJugador);
        if (distancia > radioDeteccion) return false;

        Vector3 dirAlJugador = (posJugador - transform.position).normalized;
        float angulo = Vector3.Angle(transform.forward, dirAlJugador);
        if (angulo > anguloVision / 2f) return false;

        // Raycast que ignora al propio agresor y al jugador
        Vector3 origen = transform.position + Vector3.up * 0.5f;
        Ray rayo = new Ray(origen, dirAlJugador);

        if (Physics.Raycast(rayo, out RaycastHit hit, distancia, capasObstaculo))
        {
            // Si golpea algo que NO es el jugador, hay obstáculo
            if (!hit.collider.CompareTag("Player"))
                return false;
        }

        return true;
    }

    public bool DetectarLuz(float intensidad, Vector3 posicionLuz)
    {
        
        repelido = true;
        timerRepelido = 0f;
        CambiarEstadoAgresor(EstadoAgresor.PATRULLANDO);
        return true;
    }

    private bool JugadorEnRango()
    {
        return Vector3.Distance(transform.position, objetivoJugador.position) <= radioDeteccion * 0.3f;
    }

    // -------------------------------------------------------
    // CAMBIO DE ESTADO
    // -------------------------------------------------------
    public void CambiarEstadoAgresor(EstadoAgresor nuevoEstado)
    {
        if (estadoActual == nuevoEstado) return;
        estadoActual = nuevoEstado;

        switch (estadoActual)
        {
            case EstadoAgresor.PATRULLANDO:
                agente.speed = velocidadPatrulla;
                timerAlerta = 0f;
                esperandoEnWaypoint = false; 
                if (waypoints.Length > 0)
                {
                    waypointActual = (waypointActual + 1) % waypoints.Length; 
                    agente.SetDestination(waypoints[waypointActual].position);
                }
                OnAgresorVuelveAPatrullar?.Invoke();
                break;

            case EstadoAgresor.ALERTA:
                agente.speed = velocidadPatrulla;
                timerAlerta = 0f;
                OnAgresorAlerta?.Invoke();
                break;

            case EstadoAgresor.PERSIGUIENDO:
                agente.speed = velocidadPersecucion;
                OnJugadorDetectado?.Invoke();
                break;
        }
    }

    private void Alertarse(Vector3 posicion)
    {
        ultimaPosicionSospechosa = posicion;
        CambiarEstadoAgresor(EstadoAgresor.ALERTA);
    }

    // -------------------------------------------------------
    // GIZMOS (para ver la detección en el editor)
    // -------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        // Radio de detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);

        // Cono de visión
        Gizmos.color = Color.yellow;
        Vector3 limiteIzq = Quaternion.Euler(0, -anguloVision / 2f, 0) * transform.forward;
        Vector3 limiteDer = Quaternion.Euler(0, anguloVision / 2f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, limiteIzq * radioDeteccion);
        Gizmos.DrawRay(transform.position, limiteDer * radioDeteccion);

        // Distancia de ataque
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}