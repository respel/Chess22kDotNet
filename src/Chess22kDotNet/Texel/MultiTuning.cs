using Chess22kDotNet.JavaWrappers;

namespace Chess22kDotNet.Texel
{
    public class MultiTuning : Tuning
    {
        private readonly float[] _floatValues;

        public MultiTuning(float[] floatValues, string name) : base(new int[floatValues.Length], 1, name, false)
        {
            _floatValues = floatValues;

            for (var i = 0; i < floatValues.Length; i++)
            {
                Values[i] = (int)(floatValues[i] * 10);
                OrgValues[i] = (int)(floatValues[i] * 10);
            }
        }

        public override void AddStep(int i)
        {
            Values[i] += 1;
            _floatValues[i] += 0.1f;
        }

        public override void RemoveStep(int i)
        {
            Values[i] -= 1;
            _floatValues[i] -= 0.1f;
        }

        public override string ToString()
        {
            return Name + ": " + Arrays.ToString(_floatValues);
        }

        public override void RestoreValues()
        {
            for (var i = 0; i < Values.Length; i++) _floatValues[i] = (float)OrgValues[i] / 10;
        }

        public override void ClearValues()
        {
            for (var i = 0; i < Values.Length; i++)
            {
                Values[i] = 10;
                _floatValues[i] = 1.0f;
            }
        }
    }
}