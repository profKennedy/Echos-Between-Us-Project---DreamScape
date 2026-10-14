using UnityEngine;

public class ReceptorMiradaFantasma : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float perdidaPorSegundo = 15f;
    [SerializeField] private float recuperacionPorSegundo = 5f;
    [Tooltip("Cuánto tiempo sigue contando como 'mirada' después del último RecibirMirada()")]
    [SerializeField] private float toleranciaMirada = 0.1f;

    private NivelDeAutoestima _nivelAutoestimaDani;
    private float _tiempoUltimaMirada = float.NegativeInfinity;

    private void Awake()
    {
        _nivelAutoestimaDani = GetComponentInParent<NivelDeAutoestima>();
        if (_nivelAutoestimaDani == null)
            Debug.LogError("ReceptorMiradaFantasma: no encontré NivelDeAutoestima en el padre.", this);
    }

    private void Update()
    {
        if (_nivelAutoestimaDani == null) return;

        bool siendoMirada = Time.time - _tiempoUltimaMirada <= toleranciaMirada;

        if (siendoMirada)
            _nivelAutoestimaDani.ReducirAutoestima(perdidaPorSegundo * Time.deltaTime);
        else
            _nivelAutoestimaDani.RecuperarAutoestima(recuperacionPorSegundo * Time.deltaTime);
    }

    // Los fantasmas llaman esto con su Raycast (en Update o FixedUpdate, da igual)
    public void RecibirMirada()
    {
        _tiempoUltimaMirada = Time.time;
    }
}