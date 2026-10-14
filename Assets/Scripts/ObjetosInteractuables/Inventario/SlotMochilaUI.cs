using System;
using UnityEngine;
using UnityEngine.UI;

public class SlotMochilaUI : MonoBehaviour
{
    [SerializeField] private Image icono;
    [SerializeField] private Button boton;

    public event Action<int> OnSlotSeleccionado;

    private int _indice;

    private void Awake()
    {
        boton.onClick.AddListener(() => OnSlotSeleccionado?.Invoke(_indice));
    }

    public void Inicializar(int indice)
    {
        _indice = indice;
    }

    // item == null => slot vacío
    public void Mostrar(DatosItem item)
    {
        bool hayItem = item != null;
        icono.enabled = hayItem;
        icono.sprite = hayItem ? item.icono : null;
    }
}