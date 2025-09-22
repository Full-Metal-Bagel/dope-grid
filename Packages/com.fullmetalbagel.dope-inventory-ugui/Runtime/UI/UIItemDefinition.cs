using System;
using UnityEngine;

namespace DopeGrid.Inventory
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/New Item")]
    public class UIItemDefinition : ScriptableObject
    {
        [field: SerializeField, HideInInspector] public string Id { get; private set; } = Guid.NewGuid().ToString();
        [field: SerializeField, EditorGridShapeReferenceImage(nameof(Image))] public EditorGridShape Shape { get; private set; }
        [field: SerializeField] public Sprite Image { get; private set; } = null!;
    }
}
