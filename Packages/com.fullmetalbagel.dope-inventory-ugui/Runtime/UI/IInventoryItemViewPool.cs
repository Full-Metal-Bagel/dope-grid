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
        createFunc: () => new GameObject("image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>(),
        actionOnRelease: image =>
        {
            image.gameObject.SetActive(false);
            image.transform.SetParent(s_root!.Value.transform);

            // Reset image properties to default state
            image.sprite = null;
            image.color = Color.white;
            image.raycastTarget = false;
            image.preserveAspect = false;

            // Reset transform properties
            var rectTransform = (RectTransform)image.transform;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localEulerAngles = Vector3.zero;
            rectTransform.localScale = Vector3.one;
        },
        actionOnDestroy: image => GameObject.Destroy(image.gameObject)
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
