using System;
using UnityEngine;

namespace DopeGrid;

[Serializable]
public struct EditorGridShape
{
    [field: SerializeField] public int Width { get; private set; }
    [field: SerializeField] public int Height { get; private set; }
    [field: SerializeField] public bool[] Shape { get; private set; }
}