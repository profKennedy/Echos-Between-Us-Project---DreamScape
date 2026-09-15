using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GestorSombras : MonoBehaviour
{
    public List<ObjetoProyector> objetosProyectores = new List<ObjetoProyector>();
    public List<PlataformaSombra> plataformasActivas = new List<PlataformaSombra>();

    public float intervaloActualizacion = 0.1f;
    private float tiempoTemporizador = 0f;

    // Hace Referencia al controlador de la linterna para el Action OnLuzCambiada
    public ControladorLinterna linterna;

    private void Start()
    {
        Inicializar();
    }

    public void Inicializar()
    {
        SuscribirseALinterna();
        // aqui se puede acaparar la lista buscando objetos en la escena si es que no se asignaron en el inspector
    }

    private void Update()
    {
        if (linterna != null && linterna.EstaEncendida())
        {
            tiempoTemporizador += Time.deltaTime;
            if (tiempoTemporizador >= intervaloActualizacion)
            {
                RecalcularSombras(linterna.transform.forward);
                tiempoTemporizador = 0f;
            }
        }
    }

    public void RecalcularSombras(Vector3 dirLuz)
    {
        // Usar Raycasts o colisiones para ver qué 'ObjetoProyector' está siendo iluminado
        // y llamar a su método AlIluminar() o AlDejarDeIluminar().
    }

    public void CrearPlataformaSombra(ObjetoProyector origen)
    {
        if (!plataformasActivas.Contains(origen.sombraGenerada))
        {
            plataformasActivas.Add(origen.sombraGenerada);
            origen.sombraGenerada.Activar();
        }
    }

    public void DestruirPlataformaSombra(ObjetoProyector origen)
    {
        if (plataformasActivas.Contains(origen.sombraGenerada))
        {
            origen.sombraGenerada.Desactivar();
            plataformasActivas.Remove(origen.sombraGenerada);
        }
    }

    public void DisiparNiebla(Vector3 posicion, float radio)
    {
        // lógica para comunicarse con 'GestorNiebla' y reducir la niebla
        Debug.Log("Disipando niebla en: " + posicion + " con radio: " + radio);
    }

    public void SuscribirseALinterna()
    {
        if (linterna != null)
        {
            linterna.OnLuzCambiada += (estaEncendida) =>
            {
                if (!estaEncendida)
                {
                    // si se apaga la luz, desactivar todas las plataformas
                    foreach (var proyector in objetosProyectores)
                    {
                        DestruirPlataformaSombra(proyector);
                    }
                }
            };
        }
    }
}