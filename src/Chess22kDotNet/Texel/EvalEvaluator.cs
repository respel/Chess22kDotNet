using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.Texel
{
    public class EvalEvaluator
    {
        private const int NumberOfThreads = 16;
        private static readonly ErrorCalculator[] Workers = new ErrorCalculator[NumberOfThreads];

        public static async Task Main()
        {
            // read all fens, including score
            var fens = Tuner.LoadFens("d:\\backup\\chess\\epds\\quiet-labeled.epd", true, false);
            Console.WriteLine("Fens found : " + fens.Count);

            // init workers
            ChessBoardInstances.Init(NumberOfThreads);
            ThreadData.InitInstances(NumberOfThreads);
            for (var i = 0; i < NumberOfThreads; i++)
                Workers[i] = new ErrorCalculator(ChessBoardInstances.Get(i), ThreadData.GetInstance(i));

            // add fens to workers
            var workerIndex = 0;
            foreach (var (key, value) in fens)
            {
                Workers[workerIndex].AddFenWithScore(key, value);
                workerIndex = workerIndex == NumberOfThreads - 1 ? 0 : workerIndex + 1;
            }

            // get tuned values
            var tuningObjects = Tuner.GetTuningObjects();

            // tune
            await Eval(tuningObjects);
            Console.WriteLine("Done");
        }

        private static async Task Eval(IEnumerable<Tuning> tuningObjects)
        {
            var bestError = await CalculateErrorMultiThreaded();
            Console.WriteLine($"{bestError} - org");
            foreach (var tuningObject in tuningObjects)
            {
                tuningObject.ClearValues();
                EvalConstants.InitMgEg();
                for (var i = 0; i < NumberOfThreads; i++) ThreadData.GetInstance(i).ClearCaches();

                var newError = await CalculateErrorMultiThreaded();
                Console.WriteLine($"{newError} - {tuningObject.Name}");
                tuningObject.RestoreValues();
            }
        }

        private static async Task<double> CalculateErrorMultiThreaded()
        {
            var list = new List<Task<double>>();
            for (var i = 0; i < NumberOfThreads; i++)
            {
                var i1 = i;
                var submit = Task.Run(() => Workers[i1].Call());
                list.Add(submit);
            }

            double totalError = 0;
            // now retrieve the result
            foreach (var task in list)
                try
                {
                    totalError += await task;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            return totalError / NumberOfThreads;
        }
    }
}