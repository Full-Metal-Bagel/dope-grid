using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DopeGrid.Inventory;

public static class Extension
{
    [Pure, MustUseReturnValue]
    public static int2 GetGridPosition(this InventoryView inventoryView, int2 shapeSize, Vector2 worldPosition)
    {
        // Convert from Canvas coordinates (center pivot) to InventoryView coordinates (top-left pivot)
        var inventoryRect = inventoryView.RectTransform;
        var localPosInInventory = inventoryRect.InverseTransformPoint(worldPosition);

        // Account for the dragging ghost having center pivot - convert to top-left corner
        // The ghost position represents the center of the rotated item, but we need the top-left of the grid shape
        var rotatedSize = new Vector2(shapeSize.x * inventoryView.CellSize.x, shapeSize.y * inventoryView.CellSize.y);
        var topLeftPos = localPosInInventory - new Vector3(rotatedSize.x * 0.5f, -rotatedSize.y * 0.5f, 0f);

        // Convert to top-left origin from the inventory view's pivot
        var rect = inventoryRect.rect;
        var size = rect.size;
        var left = -inventoryRect.pivot.x * size.x;
        var top = (1 - inventoryRect.pivot.y) * size.y;
        var fromTopLeft = new Vector2(topLeftPos.x - left, top - topLeftPos.y);

        var gridX = Mathf.RoundToInt(fromTopLeft.x / inventoryView.CellSize.x);
        var gridY = Mathf.RoundToInt(fromTopLeft.y / inventoryView.CellSize.y);

        return new int2(gridX, gridY);
    }

    [Pure]
    public static bool TryGetPointerLocalTopLeft(this RectTransform rectTransform, PointerEventData eventData, out Vector2 fromTopLeft)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var local))
        {
            fromTopLeft = default;
            return false;
        }

        var rect = rectTransform.rect;
        var size = rect.size;
        var left = -rectTransform.pivot.x * size.x;
        var top = (1 - rectTransform.pivot.y) * size.y;
        fromTopLeft = new Vector2(local.x - left, top - local.y);
        return true;
    }

    [Pure]
    public static bool TryGetPointerLocalInCanvas(this Canvas canvas, PointerEventData eventData, out Vector2 canvasLocal)
    {
        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        return RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, eventData.position, cam, out canvasLocal);
    }
}
