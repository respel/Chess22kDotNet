using System.Diagnostics;

namespace Chess22kDotNet
{
    public static class Assert
    {
        public static void IsTrue(bool condition)
        {
            Debug.Assert(condition);
        }

        public static void IsTrue(bool condition, string message)
        {
            Debug.Assert(condition, message);
        }
    }
}