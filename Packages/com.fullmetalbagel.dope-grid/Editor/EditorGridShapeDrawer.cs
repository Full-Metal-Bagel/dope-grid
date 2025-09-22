using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DopeGrid.Editor
{
    [CustomPropertyDrawer(typeof(EditorGridShape))]
    public class EditorGridShapeDrawer : PropertyDrawer
    {
        private const float ButtonSize = 20f;
        private const float Spacing = 2f;
        private const float HeaderHeight = 20f;

        private static readonly Dictionary<string, (int width, int height)> _previousDimensions = new Dictionary<string, (int, int)>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var widthProp = property.FindPropertyRelative($"<{nameof(EditorGridShape.Width)}>k__BackingField");
            var heightProp = property.FindPropertyRelative($"<{nameof(EditorGridShape.Height)}>k__BackingField");
            var shapeProp = property.FindPropertyRelative($"<{nameof(EditorGridShape.Shape)}>k__BackingField");

            position.height = HeaderHeight;
            EditorGUI.LabelField(position, label, EditorStyles.boldLabel);
            position.y += HeaderHeight + Spacing;

            var halfWidth = (position.width - Spacing) * 0.5f;
            var widthRect = new Rect(position.x, position.y, halfWidth, HeaderHeight);
            var heightRect = new Rect(position.x + halfWidth + Spacing, position.y, halfWidth, HeaderHeight);

            // Store current dimensions before any changes
            string propertyKey = property.propertyPath;
            if (!_previousDimensions.ContainsKey(propertyKey))
            {
                _previousDimensions[propertyKey] = (widthProp.intValue, heightProp.intValue);
            }

            EditorGUI.BeginChangeCheck();

            var newWidth = EditorGUI.IntField(widthRect, "Width", widthProp.intValue);
            var newHeight = EditorGUI.IntField(heightRect, "Height", heightProp.intValue);

            newWidth = Mathf.Max(1, newWidth);
            newHeight = Mathf.Max(1, newHeight);

            bool dimensionsChanged = newWidth != widthProp.intValue || newHeight != heightProp.intValue;

            if (dimensionsChanged)
            {
                var oldDimensions = _previousDimensions[propertyKey];
                ResizeShapeArray(shapeProp, oldDimensions.width, oldDimensions.height, newWidth, newHeight);
                widthProp.intValue = newWidth;
                heightProp.intValue = newHeight;
                _previousDimensions[propertyKey] = (newWidth, newHeight);
            }

            position.y += HeaderHeight + Spacing * 2;
            position.y += HeaderHeight + Spacing * 2;

            if (shapeProp.arraySize != newWidth * newHeight)
            {
                var oldDimensions = _previousDimensions[propertyKey];
                ResizeShapeArray(shapeProp, oldDimensions.width, oldDimensions.height, newWidth, newHeight);
                _previousDimensions[propertyKey] = (newWidth, newHeight);
            }

            var buttonWidth = position.width / 4f - Spacing * 0.75f;
            var fillRect = new Rect(position.x, position.y, buttonWidth, HeaderHeight);
            var clearRect = new Rect(position.x + buttonWidth + Spacing, position.y, buttonWidth, HeaderHeight);
            var invertRect = new Rect(position.x + (buttonWidth + Spacing) * 2, position.y, buttonWidth, HeaderHeight);
            var trimRect = new Rect(position.x + (buttonWidth + Spacing) * 3, position.y, buttonWidth, HeaderHeight);

            if (GUI.Button(fillRect, "Fill All"))
            {
                for (int i = 0; i < shapeProp.arraySize; i++)
                {
                    shapeProp.GetArrayElementAtIndex(i).boolValue = true;
                }
                property.serializedObject.ApplyModifiedProperties();
            }

            if (GUI.Button(clearRect, "Clear All"))
            {
                for (int i = 0; i < shapeProp.arraySize; i++)
                {
                    shapeProp.GetArrayElementAtIndex(i).boolValue = false;
                }
                property.serializedObject.ApplyModifiedProperties();
            }

            if (GUI.Button(invertRect, "Invert"))
            {
                for (int i = 0; i < shapeProp.arraySize; i++)
                {
                    var element = shapeProp.GetArrayElementAtIndex(i);
                    element.boolValue = !element.boolValue;
                }
                property.serializedObject.ApplyModifiedProperties();
            }

            if (GUI.Button(trimRect, "Trim"))
            {
                TrimShape(widthProp, heightProp, shapeProp);
                property.serializedObject.ApplyModifiedProperties();
            }

            position.y += HeaderHeight + Spacing * 2;

            var gridStartX = position.x + (position.width - newWidth * (ButtonSize + Spacing)) * 0.5f;

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    var index = y * newWidth + x;
                    var buttonRect = new Rect(
                        gridStartX + x * (ButtonSize + Spacing),
                        position.y + y * (ButtonSize + Spacing),
                        ButtonSize,
                        ButtonSize
                    );

                    if (index < shapeProp.arraySize)
                    {
                        var elementProp = shapeProp.GetArrayElementAtIndex(index);
                        bool isActive = elementProp.boolValue;

                        var cellColor = isActive
                            ? new Color(0.3f, 0.7f, 0.3f, 0.8f)
                            : new Color(0.3f, 0.3f, 0.3f, 0.5f);

                        EditorGUI.DrawRect(buttonRect, cellColor);

                        if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
                        {
                            elementProp.boolValue = !elementProp.boolValue;
                        }
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var widthProp = property.FindPropertyRelative($"<{nameof(EditorGridShape.Width)}>k__BackingField");
            var heightProp = property.FindPropertyRelative($"<{nameof(EditorGridShape.Height)}>k__BackingField");

            int width = widthProp?.intValue ?? 1;
            int height = heightProp?.intValue ?? 1;

            float totalHeight = HeaderHeight; // Label
            totalHeight += HeaderHeight + Spacing; // Width/Height fields
            totalHeight += HeaderHeight + Spacing * 2; // Utility buttons
            totalHeight += HeaderHeight + Spacing * 2; // Space before grid
            totalHeight += height * (ButtonSize + Spacing); // Grid

            return totalHeight;
        }

        private void ResizeShapeArray(SerializedProperty shapeProp, int oldWidth, int oldHeight, int newWidth, int newHeight)
        {
            int newSize = newWidth * newHeight;
            int oldSize = shapeProp.arraySize;

            if (newSize != oldSize)
            {
                // Store old values in 2D array
                bool[,] oldGrid = new bool[oldHeight, oldWidth];
                for (int y = 0; y < oldHeight; y++)
                {
                    for (int x = 0; x < oldWidth; x++)
                    {
                        int index = y * oldWidth + x;
                        if (index < oldSize)
                        {
                            oldGrid[y, x] = shapeProp.GetArrayElementAtIndex(index).boolValue;
                        }
                    }
                }

                // Resize array
                shapeProp.arraySize = newSize;

                // Copy old values to new grid, preserving positions
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        int index = y * newWidth + x;
                        bool value = false;

                        // If within old bounds, copy the value
                        if (x < oldWidth && y < oldHeight)
                        {
                            value = oldGrid[y, x];
                        }

                        shapeProp.GetArrayElementAtIndex(index).boolValue = value;
                    }
                }
            }
        }

        private void TrimShape(SerializedProperty widthProp, SerializedProperty heightProp, SerializedProperty shapeProp)
        {
            int width = widthProp.intValue;
            int height = heightProp.intValue;

            if (shapeProp.arraySize == 0 || width == 0 || height == 0)
                return;

            // Read current shape
            bool[,] grid = new bool[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    if (index < shapeProp.arraySize)
                    {
                        grid[y, x] = shapeProp.GetArrayElementAtIndex(index).boolValue;
                    }
                }
            }

            // Find bounds of non-empty cells
            int minX = width, maxX = -1;
            int minY = height, maxY = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[y, x])
                    {
                        minX = Mathf.Min(minX, x);
                        maxX = Mathf.Max(maxX, x);
                        minY = Mathf.Min(minY, y);
                        maxY = Mathf.Max(maxY, y);
                    }
                }
            }

            // If empty, set to 1x1 grid
            if (maxX < 0 || maxY < 0)
            {
                widthProp.intValue = 1;
                heightProp.intValue = 1;
                shapeProp.arraySize = 1;
                shapeProp.GetArrayElementAtIndex(0).boolValue = false;
                return;
            }

            // Calculate new dimensions
            int newWidth = maxX - minX + 1;
            int newHeight = maxY - minY + 1;

            // Update dimensions
            widthProp.intValue = newWidth;
            heightProp.intValue = newHeight;
            shapeProp.arraySize = newWidth * newHeight;

            // Copy trimmed data
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int newIndex = y * newWidth + x;
                    int oldY = y + minY;
                    int oldX = x + minX;
                    shapeProp.GetArrayElementAtIndex(newIndex).boolValue = grid[oldY, oldX];
                }
            }
        }
    }
}