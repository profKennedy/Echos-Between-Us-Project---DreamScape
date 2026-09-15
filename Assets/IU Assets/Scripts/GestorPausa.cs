using UnityEngine;
using UnityEngine.SceneManagement; // Por si quieren reiniciar o ir al menú principal

public class MenuPausa : MonoBehaviour
{
    [Header("UI del Menú")]
    [SerializeField] private GameObject canvasPausa;

    private bool _juegoPausado = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_juegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Pausar()
    {
        _juegoPausado = true;

        if (canvasPausa != null) canvasPausa.SetActive(true);
        Time.timeScale = 0f;

        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        _juegoPausado = false;

        if (canvasPausa != null) canvasPausa.SetActive(false);

        Time.timeScale = 1f;

        // Despausa los sonidos
        AudioListener.pause = false;

        // Opcional: Volver a bloquear el cursor si el juego lo requiere al jugar
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirAlMenu(string nombreMenu)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(nombreMenu);
    }

}