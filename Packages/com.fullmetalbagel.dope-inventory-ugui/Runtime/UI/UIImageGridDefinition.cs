using UnityEngine;

namespace DopeGrid.Inventory
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/New Item")]
    public class UIImageGridDefinition : ScriptableObject
    {
        [field: SerializeField, EditorGridShapeReferenceImage(nameof(Image))] public EditorGridShape Shape { get; private set; }
        [field: SerializeField] public Sprite Image { get; private set; } = null!;
    }
}
