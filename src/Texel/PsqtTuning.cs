using System;
using System.Text;
using Chess22kDotNet.Eval;
using Chess22kDotNet.JavaWrappers;

namespace Chess22kDotNet.Texel
{
    public class PsqtTuning : Tuning
    {
        private readonly int[][] _psqtValues;

        public PsqtTuning(int[][] psqtValues, int step, string name) : base(psqtValues[ChessConstants.White], step,
            name, true)
        {
            _psqtValues = psqtValues;
            OrgValues = new int[64];
            Array.Copy(psqtValues[ChessConstants.White], 0, OrgValues, 0, 64);
        }

        public PsqtTuning(int[][] psqtValues, int step, string name, bool pawnPsqt) : base(
            psqtValues[ChessConstants.White], step, name, true, 0, 1, 2, 3, 4, 5, 6, 7, 56, 57, 58, 59, 60, 61, 62, 63)
        {
            _psqtValues = psqtValues;
            OrgValues = new int[64];
            Array.Copy(psqtValues[ChessConstants.White], 0, OrgValues, 0, 64);
        }

        public override int GetNumberOfTunedValues()
        {
            return base.GetNumberOfTunedValues() / 2;
        }

        public override void PrintNewValues()
        {
            if (ShowAverage)
            {
                var sum = 0;
                for (var i = 0; i < 64; i++)
                {
                    sum += _psqtValues[ChessConstants.White][i];
                }

                Console.WriteLine(Name + ": (" + sum / 64 + ")" +
                                  GetArrayFriendlyFormatted(_psqtValues[ChessConstants.White]));
            }
            else
            {
                Console.WriteLine(Name + ": " + GetArrayFriendlyFormatted(_psqtValues[ChessConstants.White]));
            }
        }

        public override string ToString()
        {
            if (!ShowAverage) return Name + ": " + Arrays.ToString(_psqtValues[ChessConstants.White]);
            var sum = 0;
            for (var i = 0; i < 64; i++)
            {
                sum += _psqtValues[ChessConstants.White][i];
            }

            return Name + ": (" + sum / 64 + ")" + Arrays.ToString(_psqtValues[ChessConstants.White]);
        }

        public override void AddStep(int i)
        {
            // add to white
            _psqtValues[ChessConstants.White][i] += Step;

            // add to white mirrored
            _psqtValues[ChessConstants.White][EvalConstants.MirroredLeftRight[i]] += Step;

            // add to black
            _psqtValues[ChessConstants.Black][EvalConstants.MirroredUpDown[i]] -= Step;

            // add to black mirrored
            _psqtValues[ChessConstants.Black][EvalConstants.MirroredLeftRight[EvalConstants.MirroredUpDown[i]]] -=
                Step;
        }

        public override void RemoveStep(int i)
        {
            // remove from white
            _psqtValues[ChessConstants.White][i] -= Step;

            // remove from white mirrored
            _psqtValues[ChessConstants.White][EvalConstants.MirroredLeftRight[i]] -= Step;

            // remove from black
            _psqtValues[ChessConstants.Black][EvalConstants.MirroredUpDown[i]] += Step;

            // remove from black mirrored
            _psqtValues[ChessConstants.Black][EvalConstants.MirroredLeftRight[EvalConstants.MirroredUpDown[i]]] +=
                Step;
        }

        public override int NumberOfParameters()
        {
            return 64;
        }

        public override bool Skip(int i)
        {
            return SkipValues.Contains(i);
        }

        public static string GetArrayFriendlyFormatted(int[] values)
        {
            var sb = new StringBuilder("\n");
            for (var i = 7; i >= 0; i--)
            {
                sb.Append(" ");
                for (var j = 7; j >= 0; j--)
                {
                    sb.Append($"{values[i * 8 + j],3}").Append(",");
                }

                sb.Append("\n");
            }

            sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }

        public override void ClearValues()
        {
            for (var i = 0; i < 64; i++)
            {
                _psqtValues[ChessConstants.White][i] = 0;
                _psqtValues[ChessConstants.Black][i] = 0;
            }
        }

        public override void RestoreValues()
        {
            for (var i = 0; i < 64; i++)
            {
                _psqtValues[ChessConstants.White][i] = OrgValues[i];
            }

            for (var i = 0; i < 64; i++)
            {
                _psqtValues[ChessConstants.Black][i] = -_psqtValues[ChessConstants.White][63 - i];
            }
        }

        public override bool IsUpdated()
        {
            for (var i = 0; i < 64; i++)
            {
                if (OrgValues[i] != _psqtValues[ChessConstants.White][i])
                {
                    return true;
                }
            }

            return false;
        }
    }
}