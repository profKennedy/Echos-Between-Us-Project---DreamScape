using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeAlEntrar : MonoBehaviour
{
    public float duracionFade = 1.5f;
    private Image faderImage;

    private void Awake()
    {
        faderImage = GetComponent<Image>();
        Color c = faderImage.color;
        c.a = 1f;
        faderImage.color = c;
        faderImage.raycastTarget = true;
    }

    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        yield return new WaitForSecondsRealtime(0.15f);

        Color color = faderImage.color;
        float tiempo = 0f;

        while (tiempo < duracionFade)
        {
            tiempo += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float progreso = Mathf.Clamp01(tiempo / duracionFade);
            color.a = Mathf.SmoothStep(1f, 0f, progreso);
            faderImage.color = color;
            yield return null;
        }

        color.a = 0f;
        faderImage.color = color;
        faderImage.raycastTarget = false;
    }
}