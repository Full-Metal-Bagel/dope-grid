using System;
using UnityEngine;

namespace DopeGrid;

[AttributeUsage(AttributeTargets.Field)]
public class EditorGridShapeReferenceImageAttribute : Attribute
{
    public string MemberName { get; }
    public EditorGridShapeReferenceImageAttribute(string memberName) => MemberName = memberName;
}

[Serializable]
public struct EditorGridShape
{
    [field: SerializeField] public int Width { get; private set; }
    [field: SerializeField] public int Height { get; private set; }
    [field: SerializeField] public bool[] Shape { get; private set; }
}
