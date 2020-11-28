using System;
using System.Collections.Generic;
using System.Linq;
using Chess22kDotNet.JavaWrappers;

namespace Chess22kDotNet.Texel
{
    public class Tuning
    {
        public readonly string Name;
        public readonly bool ShowAverage;
        protected readonly List<int> SkipValues;
        protected readonly int Step;
        protected readonly int[] Values;
        protected int[] OrgValues;

        public Tuning(int[] values, int step, string name, params int[] skipValues) : this(values, step, name, false,
            skipValues)
        {
        }

        protected Tuning(int[] values, int step, string name, bool showAverage, params int[] skipValues)
        {
            Values = values;
            Step = step;
            ShowAverage = showAverage;
            while (name.Length < 20) name += " ";

            Name = name;
            SkipValues = skipValues.ToList();
            if (values == null) return;
            OrgValues = new int[values.Length];
            Array.Copy(values, 0, OrgValues, 0, values.Length);
        }

        public virtual int GetNumberOfTunedValues()
        {
            return Values.Length - SkipValues.Count;
        }

        public virtual void PrintNewValues()
        {
            Console.WriteLine(ToString());
        }

        public override string ToString()
        {
            if (ShowAverage) return Name + ": " + Arrays.ToString(Values) + " (" + GetAverage() + ")";

            return Name + ": " + Arrays.ToString(Values);
        }

        public int GetAverage()
        {
            var sum = Values.Sum();
            return sum / Values.Length;
        }

        public virtual void AddStep(int i)
        {
            Values[i] += Step;
        }

        public virtual void RemoveStep(int i)
        {
            Values[i] -= Step;
        }

        public virtual int NumberOfParameters()
        {
            return Values.Length;
        }

        public virtual bool Skip(int i)
        {
            return SkipValues.Contains(i);
        }

        public virtual bool IsUpdated()
        {
            return OrgValues.Where((t, i) => t != Values[i]).Any();
        }

        public virtual void ClearValues()
        {
            for (var i = 0; i < Values.Length; i++) Values[i] = 0;
        }

        public virtual void RestoreValues()
        {
            for (var i = 0; i < Values.Length; i++) Values[i] = OrgValues[i];
        }
    }
}