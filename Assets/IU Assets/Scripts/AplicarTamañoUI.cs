using UnityEngine;

public class AplicarTamanoUI : MonoBehaviour
{
    private Vector3 escalaBase;

    private void Awake()
    {
        // Guarda la escala original que configuraste en el Inspector
        escalaBase = transform.localScale;
    }

    private void OnEnable()
    {
        // Aplica el tamaño guardado apenas aparece en escena
        float escalaGuardada = PlayerPrefs.GetFloat(ControladorTamanoUI.CLAVE_ESCALA, 1f);
        ActualizarTamano(escalaGuardada);

        // Se suscribe para cambiar en vivo si mueven el slider en la pausa
        ControladorTamanoUI.OnEscalaCambiada += ActualizarTamano;
    }

    private void OnDisable()
    {
        ControladorTamanoUI.OnEscalaCambiada -= ActualizarTamano;
    }

    private void ActualizarTamano(float factor)
    {
        transform.localScale = escalaBase * factor;
    }
}