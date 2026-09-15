using System.Collections; // Sumamos esto para poder usar las pausas de tiempo
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    public void EmpezarJuego()
    {
        StartCoroutine(EsperaParaJugar());
    }

    private IEnumerator EsperaParaJugar()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Nivel_1");
    }

    public void SalirDelJuego()
    {
        StartCoroutine(EsperaParaSalir());
    }

    private IEnumerator EsperaParaSalir()
    {
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }
}