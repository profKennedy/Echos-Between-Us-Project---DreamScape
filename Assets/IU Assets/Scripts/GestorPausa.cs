using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [Header("UI del Menú")]
    [SerializeField] private GameObject canvasPausa;

    [Header("Otras interfaces")]
    [SerializeField] private InterfazMochila interfazMochila;
    [SerializeField] private InterfazMobile interfazMobile;

    private bool _juegoPausado = false;

    public bool EstaPausado => _juegoPausado;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_juegoPausado)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        // No permitir abrir pausa mientras la mochila esté abierta.
        if (_juegoPausado)
            return;

        if (interfazMochila != null && interfazMochila.EstaAbierta)
            return;

        _juegoPausado = true;

        if (canvasPausa != null)
            canvasPausa.SetActive(true);

        if (interfazMobile != null)
            interfazMobile.MostrarControles(false);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        _juegoPausado = false;

        if (canvasPausa != null)
            canvasPausa.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (interfazMobile != null)
            interfazMobile.MostrarControles(true);

        // Conservamos el comportamiento original del cursor.
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void SalirAlMenu(string nombreMenu)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(nombreMenu);
    }
}