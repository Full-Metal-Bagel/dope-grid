// using System;
// using DopeGrid.Native;
// using Unity.Mathematics;
// using UnityEngine;
//
// namespace DopeGrid.Inventory;
//
// public class DraggingItem
// {
//     public InventoryItemInstanceId InstanceId { get; }
//     public ItemDefinition Definition { get; }
//     public Guid DefinitionId => Definition.Id;
//     public ImmutableGridShape Shape => Definition.Shape.GetRotatedShape(Rotation);
//     public RectTransform View { get; }
//     public RotationDegree Rotation { get; set; }
//
//     // public Inventory SourceInventory { get; set; } = default;
//     // public Inventory TargetInventory { get; set; } = default;
//     public int2 TargetPosition { get; set; } = default;
//     public int LastFrame { get; set; } = 0;
//
//     public DraggingItem(InventoryItemInstanceId id, ItemDefinition definition, RectTransform view, RotationDegree rotation = RotationDegree.None)
//     {
//         InstanceId = id;
//         Definition = definition;
//         View = view;
//         Rotation = rotation;
//     }
//
//     public int2 GetGridPosition(RectTransform transform, float2 cellSize)
//     {
//         var (width, height) = Shape.Bound;
//         return transform.GetGridPosition(cellSize, width, height, View.position);
//     }
// }
