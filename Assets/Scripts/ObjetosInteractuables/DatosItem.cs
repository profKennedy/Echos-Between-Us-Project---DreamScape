using UnityEngine;

// Cada ASSET de este tipo es un item distinto del juego.
// La referencia al asset ES el identificador: una puerta que pide "Llave_Puerta2"
// solo se abre con ese asset, sin strings que puedan tener un typo.
[CreateAssetMenu(fileName = "NuevoItem", menuName = "Echoes/Datos de Item")]
public class DatosItem : ScriptableObject
{
    [Header("Identidad")]
    public string nombre;
    [TextArea] public string descripcion;
    public Sprite icono;

    [Header("Mundo")]
    [Tooltip("Prefab que aparece en el mundo cuando se descarta. Obligatorio si querés poder descartarlo.")]
    public GameObject prefabMundo;

    [Header("Comportamiento")]
    [Tooltip("Si está marcado, se borra de la mochila al usarse (llaves, pilas, items de puzzle).")]
    public bool consumirAlUsar;

    [Tooltip("Qué hace al apretar 'Usar' en la mochila. Vacío = no se usa desde la mochila (ej: las llaves se usan en la puerta).")]
    public EfectoItem efecto;
}