using System.Diagnostics.CodeAnalysis;

namespace UnityEngine
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    [SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable")]
    public class Debug
    {
        [SuppressMessage("Performance", "CA1822:Mark members as static")]
        public static void Log(object message)
        {
            // Mock implementation - in tests we can just ignore logs
        }
        
        public static void LogError(object message)
        {
            // Mock implementation
        }
        
        public static void LogWarning(object message)
        {
            // Mock implementation
        }
    }
}