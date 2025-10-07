using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DopeGrid.Inventory;

public interface ISharedInventoryData
{
    [Pure, MustUseReturnValue]
    IGameInventory? GetOwner(Guid itemInstanceId);
    void SetOwner(Guid itemInstanceId, IGameInventory? inventory);

    [Pure, MustUseReturnValue]
    ImmutableGridShape GetShape(Guid itemInstanceId);

    [Pure, MustUseReturnValue]
    RotationDegree GetRotation(Guid itemInstanceId);
    void SetRotation(Guid itemInstanceId, RotationDegree rotation);
}

public class SharedInventoryData : ISharedInventoryData
{
    public Dictionary<Guid, IGameInventory?> ItemInventoryMap { get; } = new();
    public Dictionary<Guid, ImmutableGridShape> ItemShapeMap { get; } = new();
    public Dictionary<Guid, RotationDegree> ItemRotationMap { get; } = new();

    public IGameInventory? GetOwner(Guid itemInstanceId) => ItemInventoryMap.GetValueOrDefault(itemInstanceId);
    public void SetOwner(Guid itemInstanceId, IGameInventory? inventory) => ItemInventoryMap[itemInstanceId] = inventory;
    public ImmutableGridShape GetShape(Guid itemInstanceId) => ItemShapeMap[itemInstanceId];
    public RotationDegree GetRotation(Guid itemInstanceId) => ItemRotationMap[itemInstanceId];
    public void SetRotation(Guid itemInstanceId, RotationDegree rotation) => ItemRotationMap[itemInstanceId] = rotation;
}
