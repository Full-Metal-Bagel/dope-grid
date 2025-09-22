using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DopeGrid.Inventory;

public interface IInventoryItemViewPool
{
    Image Get();
    void Release(Image item);
}

public class DefaultInventoryItemViewPool : IInventoryItemViewPool
{
    private static readonly Lazy<Transform> s_root;
    private static readonly ObjectPool<Image> s_pool = new(
        createFunc: () => new GameObject("item", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>(),
        actionOnRelease: item => item.transform.SetParent(s_root!.Value.transform),
        actionOnDestroy: item => GameObject.Destroy(item.gameObject)
    );

    static DefaultInventoryItemViewPool()
    {
        s_root = new Lazy<Transform>(() =>
        {
            var root = GameObject.Find("__PoolRoot__") ?? new GameObject("__PoolRoot__");
            return root.transform;
        });
    }

    public Image Get()
    {
        return s_pool.Get();
    }

    public void Release(Image item)
    {
        s_pool.Release(item);
    }
}
