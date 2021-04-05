using System;
using Chess22kDotNet.JavaWrappers;

namespace Chess22kDotNet.Texel
{
    public class TableTuning : Tuning
    {
        private readonly int[][] _orgValues;
        private readonly int[][] _values;

        public TableTuning(int[][] values, int step, string name) : base(null, step, name)
        {
            _values = values;
            _orgValues = new int[values.Length][];
            for (var i = 0; i < values.Length; i++)
            {
                _orgValues[i] = new int[values[i].Length];
                Array.Copy(values[i], 0, _orgValues[i], 0, values[i].Length);
            }
        }

        public override string ToString()
        {
            return Name + ": " + Arrays.DeepToString(_values);
        }

        public override void PrintNewValues()
        {
            Console.WriteLine(Name + ":");
            for (var i = 0; i < _values.Length; i++)
                if (i == _values.Length - 1)
                    Console.WriteLine(Arrays.ToString(_values[i]).Replace("[", "{").Replace("]", "}"));
                else
                    Console.WriteLine(Arrays.ToString(_values[i]).Replace("[", "{").Replace("]", "}") + ",");
        }

        public override int GetNumberOfTunedValues()
        {
            return _values.Length * _values[0].Length;
        }

        public override void AddStep(int i)
        {
            _values[i / _values[0].Length][i % _values[0].Length] += Step;
        }

        public override void RemoveStep(int i)
        {
            _values[i / _values[0].Length][i % _values[0].Length] -= Step;
        }

        public override int NumberOfParameters()
        {
            return _values.Length * _values[0].Length;
        }

        public override bool IsUpdated()
        {
            for (var i = 0; i < _orgValues.Length; i++)
                for (var j = 0; j < _orgValues[0].Length; j++)
                    if (_orgValues[i][j] != _values[i][j])
                        return true;

            return false;
        }

        public override void ClearValues()
        {
            foreach (var t in _values)
                for (var j = 0; j < _orgValues[0].Length; j++)
                    t[j] = 0;
        }

        public override void RestoreValues()
        {
            for (var i = 0; i < _values.Length; i++)
                for (var j = 0; j < _orgValues[0].Length; j++)
                    _values[i][j] = _orgValues[i][j];
        }
    }
}