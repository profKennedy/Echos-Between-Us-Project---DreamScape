
using UnityEngine;

public class DialogoEsferaGuia : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private EsferaGuia esferaGuia;
    [SerializeField] private DialogoInicial sistemaDialogo;

    [Header("Diálogo de Dani")]
    [SerializeField, TextArea]
    private string mensaje =
        "¡Espera! ¿Quieres que te siga?";

    [SerializeField] private float duracion = 3f;

    private void OnEnable()
    {
        if (esferaGuia != null)
            esferaGuia.OnEsferaActivada += MostrarMensaje;
    }

    private void OnDisable()
    {
        if (esferaGuia != null)
            esferaGuia.OnEsferaActivada -= MostrarMensaje;
    }

    private void MostrarMensaje()
    {
        if (sistemaDialogo != null)
        {
            sistemaDialogo.MostrarDialogo(
                mensaje,
                duracion
            );
        }
    }
}
