using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace DopeGrid.Inventory
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InventoryView))]
    public class InventoryViewDebugOverlay : MonoBehaviour
    {
        [SerializeField] private bool _showOverlay = true;
        [SerializeField] private Color _textColor = Color.red;

        private InventoryView _view = null!;

        private RectTransform? _root;
        private readonly List<Text> _itemTexts = new();
        private readonly List<Text> _emptyTexts = new();
        private static Font? s_font;

        private void Awake()
        {
            _view = GetComponent<InventoryView>();
        }

        private void Update()
        {
            var inv = _view.InventoryModel;
            if (!inv.IsCreated)
                return;

            if (_showOverlay)
            {
                EnsureRoot();
                _root!.SetAsLastSibling();
                _root.sizeDelta = _view.RectTransform.sizeDelta;
                EnsureFont();

                UpdateEmptyCells();
                UpdateItemIndices();
            }
            else
            {
                // Restore image alphas
                SetGroupActive(_itemTexts, false);
                SetGroupActive(_emptyTexts, false);
                if (_root != null) _root.gameObject.SetActive(false);
            }
        }

        private void EnsureRoot()
        {
            if (_root != null) { _root.gameObject.SetActive(true); return; }
            var go = new GameObject("DebugOverlay", typeof(RectTransform));
            go.transform.SetParent(_view.transform, false);
            _root = (RectTransform)go.transform;
            EnsureTopLeft(_root);
        }

        private static void EnsureTopLeft(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
        }

        private void EnsureFont()
        {
            if (s_font != null) return;
            try
            {
                s_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (ArgumentException)
            {
                s_font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        private void UpdateItemIndices()
        {
            var inv = _view.InventoryModel;
            var cellSize = _view.CellSize;

            // Count needed labels across all items
            var needed = 0;
            for (int i = 0; i < inv.ItemCount; i++)
            {
                if (!inv.TryGetItem(i, out var it)) continue;
                needed += it.Shape.OccupiedSpaceCount;
            }

            EnsureTextPool(_itemTexts, needed, _root!);
            DeactivateSurplus(_itemTexts, needed);

            // Fill labels
            var idx = 0;
            var fontSize = Mathf.Max(10, Mathf.RoundToInt(Mathf.Min(cellSize.x, cellSize.y) * 0.5f));
            for (int i = 0; i < inv.ItemCount; i++)
            {
                if (!inv.TryGetItem(i, out var it)) continue;
                var shape = it.Shape;
                var origin = it.Position;
                for (int y = 0; y < shape.Height; y++)
                {
                    for (int x = 0; x < shape.Width; x++)
                    {
                        if (!shape.GetCell(x, y)) continue;
                        var t = _itemTexts[idx++];
                        var rt = (RectTransform)t.transform;
                        rt.sizeDelta = cellSize;
                        var gridPos = new int2(origin.x + x, origin.y + y);
                        rt.anchoredPosition = new Vector2(gridPos.x * cellSize.x, -(gridPos.y * cellSize.y));
                        t.fontSize = fontSize;
                        t.text = i.ToString();
                        t.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void UpdateEmptyCells()
        {
            var inv = _view.InventoryModel;
            var cellSize = _view.CellSize;
            var width = inv.Width;
            var height = inv.Height;
            var needed = math.max(0, width * height);

            EnsureTextPool(_emptyTexts, needed, _root!);
            DeactivateSurplus(_emptyTexts, needed);

            var fs = Mathf.Max(10, Mathf.RoundToInt(Mathf.Min(cellSize.x, cellSize.y) * 0.5f));
            for (int i = 0; i < needed; i++)
            {
                var x = i % width;
                var y = i / width;
                var val = inv.Grid[x, y];
                var t = _emptyTexts[i];
                var rt = (RectTransform)t.transform;
                rt.sizeDelta = cellSize;
                rt.anchoredPosition = new Vector2(x * cellSize.x, -(y * cellSize.y));
                t.fontSize = fs;
                var isEmpty = val == -1;
                t.text = isEmpty ? "-1" : string.Empty;
                t.gameObject.SetActive(isEmpty);
            }
        }

        private static void SetGroupActive(List<Text> list, bool active)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                if (t != null) t.gameObject.SetActive(active);
            }
        }

        private void EnsureTextPool(List<Text> pool, int needed, Transform parent)
        {
            while (pool.Count < needed)
            {
                pool.Add(CreateText(parent));
            }
        }

        private void DeactivateSurplus(List<Text> pool, int needed)
        {
            for (int i = needed; i < pool.Count; i++)
            {
                var t = pool[i];
                if (t != null) t.gameObject.SetActive(false);
            }
        }

        private Text CreateText(Transform parent)
        {
            var go = new GameObject("DbgText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            EnsureTopLeft(rt);
            var txt = go.GetComponent<Text>();
            txt.raycastTarget = false;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = _textColor;
            if (s_font != null) txt.font = s_font;
            return txt;
        }
    }
}
