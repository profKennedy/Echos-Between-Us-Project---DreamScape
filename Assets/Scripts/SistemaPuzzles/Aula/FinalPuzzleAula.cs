using System.Collections;
using UnityEngine;

public class FinalPuzzleAula : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject fantasmaAula;
    [SerializeField] private Puerta puertaAula;
    [SerializeField] private EfectoEncogimiento efectoEncogimiento;

    [Header("Configuracion")]
    [SerializeField] private float retrasoApertura = 1f;

    private bool puzzleCompletado = false;

    public void CompletarPuzzle()
    {
        if (puzzleCompletado)
            return;

        puzzleCompletado = true;

        StartCoroutine(SecuenciaFinal());
    }

    private IEnumerator SecuenciaFinal()
    {
        Debug.Log("Puzzle del aula completado.");

        // 1. Desaparece el fantasma.
        if (fantasmaAula != null)
            fantasmaAula.SetActive(false);

        // 2. Dani recupera su tamaño.
        if (efectoEncogimiento != null)
            efectoEncogimiento.RecuperarTamanoPorPuzzle();

        // 3. Esperamos antes de abrir la puerta.
        yield return new WaitForSeconds(retrasoApertura);

        // 4. Abrimos nuevamente la puerta.
        if (puertaAula != null)
            puertaAula.AbrirAlCompletarPuzzle();

        Debug.Log("Dani puede salir del aula.");
    }
}