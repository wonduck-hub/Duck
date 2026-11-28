using System.Diagnostics;

namespace DuckLib
{
    public static class Debug
    {
        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                System.Diagnostics.Debug.Assert(false, message);
            }
        }
    }
}
