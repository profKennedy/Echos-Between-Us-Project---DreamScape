using UnityEngine;

public class GeneradorTexturaAdaptado : MonoBehaviour
{
    [SerializeField] private Material materialNiebla;
    [SerializeField] private int resolucion = 64;

    private void Awake()
    {
        if (materialNiebla == null) return;
        materialNiebla.mainTexture = GenerarTexturaCircularSuave();
    }

    private Texture2D GenerarTexturaCircularSuave()
    {
        Texture2D tex = new Texture2D(resolucion, resolucion, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2(resolucion / 2f, resolucion / 2f);
        float radio = resolucion / 2f;

        for (int x = 0; x < resolucion; x++)
        {
            for (int y = 0; y < resolucion; y++)
            {
                float distancia = Vector2.Distance(new Vector2(x, y), centro);
                float alpha = Mathf.Clamp01(1f - (distancia / radio));
                // Curva suave para que los bordes se desvanezcan
                alpha = Mathf.Pow(alpha, 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return tex;
    }
}