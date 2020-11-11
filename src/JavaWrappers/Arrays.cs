using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Chess22kDotNet.JavaWrappers
{
    public static class Arrays
    {
        public static string ToString(IEnumerable a)
        {
            return "[" + string.Join(", ", a) + "]";
        }

        public static string DeepToString(IEnumerable<int[]> a)
        {
            return "[" + string.Join(", ", a.Select(ToString)) + "]";
        }

        public static T[] CopyOfRange<T>(T[] original, int from, int to)
        {
            var copy = new T[to - from];
            for (var i = from; i < Math.Min(to, original.Length); i++)
            {
                copy[i - from] = original[i];
            }

            return copy;
        }
    }
}