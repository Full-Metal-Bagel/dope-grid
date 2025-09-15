using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0011:Add braces", Justification = "Unity code style")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity struct compatibility")]
[assembly: SuppressMessage("Design", "CA1066:Implement IEquatable<T> when overriding Equals", Justification = "Mock implementation")]
[assembly: SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Mock implementation")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Unity compatibility")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Unity compatibility")]