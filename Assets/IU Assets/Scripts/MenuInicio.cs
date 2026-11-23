using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuInicio : MonoBehaviour
{
    [Header("Configuración del Fade")]
    public Image faderImage;
    public float duracionFade = 1.5f;

    private bool estaCambiando = false;

    private void Awake()
    {
        if (faderImage != null)
        {
            faderImage.gameObject.SetActive(true);
            Color c = faderImage.color;
            c.a = 1f;
            faderImage.color = c;
        }
    }

    private void Start()
    {
        StartCoroutine(FadeEntradaInicial());
    }

    private IEnumerator FadeEntradaInicial()
    {
        // Espera a que Unity termine el tirón inicial al cargar la escena
        yield return null;
        yield return null;
        yield return new WaitForSecondsRealtime(0.15f);

        yield return StartCoroutine(HacerFade(1f, 0f));
    }

    public void EmpezarJuego()
    {
        if (!estaCambiando)
        {
            StartCoroutine(EsperaParaJugar());
        }
    }

    private IEnumerator EsperaParaJugar()
    {
        estaCambiando = true;
        yield return StartCoroutine(HacerFade(0f, 1f));
        SceneManager.LoadScene("BOOK");
    }

    public void SalirDelJuego()
    {
        if (!estaCambiando)
        {
            StartCoroutine(EsperaParaSalir());
        }
    }

    private IEnumerator EsperaParaSalir()
    {
        estaCambiando = true;
        yield return StartCoroutine(HacerFade(0f, 1f));
        Debug.Log("Salió del juego");
        Application.Quit();
    }

    private IEnumerator HacerFade(float alphaInicial, float alphaFinal)
    {
        if (faderImage == null) yield break;

        faderImage.gameObject.SetActive(true);
        faderImage.raycastTarget = true;
        Color color = faderImage.color;
        float tiempo = 0f;

        while (tiempo < duracionFade)
        {
            // Evita saltos bruscos si un frame se traba
            tiempo += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float progreso = Mathf.Clamp01(tiempo / duracionFade);
            color.a = Mathf.SmoothStep(alphaInicial, alphaFinal, progreso);
            faderImage.color = color;
            yield return null;
        }

        color.a = alphaFinal;
        faderImage.color = color;

        if (alphaFinal == 0f)
        {
            faderImage.raycastTarget = false;
        }
    }
}