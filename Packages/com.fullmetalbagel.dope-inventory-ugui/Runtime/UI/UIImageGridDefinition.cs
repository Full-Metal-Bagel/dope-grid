using System;
using UnityEngine;

namespace DopeGrid.Inventory
{
    [CreateAssetMenu(fileName = "Item", menuName = "Inventory/New Item")]
    public class UIImageGridDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; } = null!;
        [field: SerializeField, EditorGridShapeReferenceImage(nameof(Image))] public EditorGridShape Shape { get; private set; }
        [field: SerializeField] public Sprite Image { get; private set; } = null!;

#if UNITY_EDITOR
        private void Awake()
        {
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out var guid, out long _);
            Id = guid;
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }
#endif

        public UIImageGridDefinitionData ToData() => new(Guid.Parse(Id), Shape.ToImmutableGridShape(), Image);
    }

    public class UIImageGridDefinitionData
    {
        public Guid Id { get; }
        public ImmutableGridShape Shape { get; }
        public Sprite Image { get; }

        public UIImageGridDefinitionData(Guid id, ImmutableGridShape shape, Sprite image)
        {
            Id = id;
            Shape = shape;
            Image = image;
        }
    }
}
