using UnityEngine;
using System.Collections;

public class FragmentoFinal : MonoBehaviour
{
    public GameObject canvasFinal;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvasFinal.SetActive(true);
            StartCoroutine(FinalizarJuego());
        }
    }

    IEnumerator FinalizarJuego()
    {
        yield return new WaitForSeconds(3f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}