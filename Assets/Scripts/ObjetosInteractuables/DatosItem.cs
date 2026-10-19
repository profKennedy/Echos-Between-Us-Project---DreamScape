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
    [Tooltip("Prefab que aparece en el mundo cuando se descarta. Obligatorio si querés poder descartarlo (salvo objetos únicos como la linterna).")]
    public GameObject prefabMundo;

    [Header("Comportamiento")]
    [Tooltip("Si está marcado, el item SALE de la mochila al usarse (llaves, pilas, items de puzzle, y la linterna cuando se equipa).")]
    public bool consumirAlUsar;

    [Tooltip("Qué hace al apretar el botón de acción en la mochila. Vacío = no se usa desde la mochila (ej: las llaves se usan en la puerta).")]
    public EfectoItem efecto;

    [Tooltip("Texto del botón de acción: 'Usar', 'Equipar', etc.")]
    public string textoAccion = "Usar";
}