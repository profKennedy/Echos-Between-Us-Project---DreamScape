
using System.Collections;
using UnityEngine;

public class DesvanecerObjetosAula : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float duracion = 2f;

    [SerializeField]
    private AnimationCurve curvaDesaparicion =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Transform[] objetos;
    private Vector3[] escalasOriginales;
    private bool desapareciendo;

    private void Awake()
    {
        // Obtenemos los hijos directos del grupo.
        int cantidad = transform.childCount;

        objetos = new Transform[cantidad];
        escalasOriginales = new Vector3[cantidad];

        for (int i = 0; i < cantidad; i++)
        {
            objetos[i] = transform.GetChild(i);
            escalasOriginales[i] = objetos[i].localScale;
        }
    }

    public void IniciarDesaparicion()
    {
        if (desapareciendo)
            return;

        desapareciendo = true;
        StartCoroutine(Desaparecer());
    }

    private IEnumerator Desaparecer()
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(tiempo / duracion);
            float suavizado = curvaDesaparicion.Evaluate(progreso);

            float escala = Mathf.Lerp(1f, 0f, suavizado);

            for (int i = 0; i < objetos.Length; i++)
            {
                if (objetos[i] != null)
                {
                    objetos[i].localScale =
                        escalasOriginales[i] * escala;
                }
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
