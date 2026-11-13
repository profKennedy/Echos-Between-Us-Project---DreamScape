using System.Collections;
using UnityEngine;

// Controla los fantasmas (Agresor) de la biblioteca: los deja ocultos al empezar, los hace
// aparecer cuando se llama a Activar() y los desaparece cuando se llama a Desactivar().
// No toca el script Agresor de tu compañero: solo prende/apaga sus GameObjects.
//
// Se conecta desde EncierroBiblioteca con los UnityEvents:
//   alDevolverControl -> FantasmasBiblioteca.Activar
//   alLiberar         -> FantasmasBiblioteca.Desactivar
//
// IMPORTANTE: este script tiene que estar en un GameObject que SIEMPRE esté activo
// (por ejemplo un vacío "FantasmasBiblioteca"). Los fantasmas pueden ser hijos suyos o no.
public class FantasmasBiblioteca : MonoBehaviour
{
    [Tooltip("Los Agresor de la biblioteca (los GameObject raíz de cada fantasma).")]
    [SerializeField] private GameObject[] fantasmas;

    [Header("Aparición")]
    [Tooltip("Espera después de llamar a Activar() antes de que aparezca el primero.")]
    [SerializeField] private float retrasoInicial = 0.5f;
    [Tooltip("Tiempo entre la aparición de un fantasma y el siguiente (0 = todos juntos).")]
    [SerializeField] private float intervaloEntreFantasmas = 1.5f;

    [Header("Desaparición")]
    [Tooltip("Espera después de llamar a Desactivar() antes de apagarlos.")]
    [SerializeField] private float retrasoDesaparicion = 0f;

    private Coroutine _rutina;
    private bool _activos;

    private void Awake()
    {
        // Quedan ocultos aunque en el editor hayas dejado alguno prendido.
        SetTodos(false);
    }

    // Conectar a alDevolverControl (o alEncerrar si querés que aparezcan antes).
    public void Activar()
    {
        if (_activos) return;
        _activos = true;
        Reiniciar(RutinaActivar());
    }

    // Conectar a alLiberar.
    public void Desactivar()
    {
        if (!_activos) return;
        _activos = false;
        Reiniciar(RutinaDesactivar());
    }

    private void Reiniciar(IEnumerator rutina)
    {
        if (_rutina != null) StopCoroutine(_rutina);
        _rutina = StartCoroutine(rutina);
    }

    private IEnumerator RutinaActivar()
    {
        if (retrasoInicial > 0f) yield return new WaitForSeconds(retrasoInicial);

        foreach (GameObject f in fantasmas)
        {
            if (f != null) f.SetActive(true);
            if (intervaloEntreFantasmas > 0f) yield return new WaitForSeconds(intervaloEntreFantasmas);
        }
        _rutina = null;
    }

    private IEnumerator RutinaDesactivar()
    {
        if (retrasoDesaparicion > 0f) yield return new WaitForSeconds(retrasoDesaparicion);
        SetTodos(false);
        _rutina = null;
    }

    private void SetTodos(bool activo)
    {
        foreach (GameObject f in fantasmas)
            if (f != null) f.SetActive(activo);
    }
}