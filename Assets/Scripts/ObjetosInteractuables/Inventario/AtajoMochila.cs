using UnityEngine;
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Abre/cierra la mochila con una tecla. Va en el MISMO objeto que InterfazMochila (el Canvas).
// Usa el Input Manager clásico (UnityEngine.Input) cuando está habilitado, así NO depende del paquete Input System.
// Solo si el proyecto está en "Input System Package (New)" usa el paquete.
// Es provisorio: cuando quieras, esta lógica se mueve a GestorEntradas con el mismo patrón que AlInteractuar.
public class AtajoMochila : MonoBehaviour
{
    [SerializeField] private InterfazMochila interfaz;

#if ENABLE_LEGACY_INPUT_MANAGER
    [SerializeField] private KeyCode tecla = KeyCode.Tab;
#elif ENABLE_INPUT_SYSTEM
    [SerializeField] private Key tecla = Key.Tab;
#endif

    private void Update()
    {
        if (interfaz == null) return;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(tecla))
            interfaz.Alternar();
#elif ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current[tecla].wasPressedThisFrame)
            interfaz.Alternar();
#endif
    }

    // Permite abrir y cerrar la mochila desde PC o Android.
    public void AlternarMochila()
    {
        if (interfaz == null)
        {
            Debug.LogWarning("No se asignó la InterfazMochila.");
            return;
        }

        interfaz.Alternar();
    }
}