using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Chess22kDotNet.Eval;
using Chess22kDotNet.Move;
using Chess22kDotNet.Search;

namespace Chess22kDotNet.Texel
{
    public static class Tuner
    {
        private const int NumberOfThreads = 16;

        private const int Step = 4;
        private static readonly ChessBoard Cb = ChessBoardInstances.Get(0);
        private static readonly ThreadData ThreadData = new ThreadData(0);
        private static readonly ErrorCalculator[] Workers = new ErrorCalculator[NumberOfThreads];

        private static double _orgError;
        private static double _bestError;

        public static List<Tuning> GetTuningObjects()
        {
            var tunings = new List<Tuning>
            {
                new Tuning(EvalConstants.OtherScores, Step, "Other scores"),
                new Tuning(EvalConstants.ThreatsMg, Step, "Threats mg"),
                new Tuning(EvalConstants.ThreatsEg, Step, "Threats eg"),
                new Tuning(EvalConstants.PawnScores, Step, "Pawn scores"),
                new Tuning(EvalConstants.ImbalanceScores, Step, "Imbalance scores")
            };

            // tunings.add(new Tuning(EvalConstants.PHASE, 1, "Phase", 0, 1));


            // tunings.add(new Tuning(EvalConstants.MATERIAL, STEP, "Material", 0, 1, 6));
            // tunings.add(new Tuning(EvalConstants.PINNED, STEP, "Pinned", 0));
            // tunings.add(new Tuning(EvalConstants.DISCOVERED, STEP, "Discovered", 0));
            // tunings.add(new Tuning(EvalConstants.DOUBLE_ATTACKED, STEP, "Double attacked"));
            // tunings.add(new Tuning(EvalConstants.NIGHT_PAWN, STEP, "Night pawn"));
            // tunings.add(new Tuning(EvalConstants.ROOK_PAWN, STEP, "Rook pawn"));
            // tunings.add(new Tuning(EvalConstants.BISHOP_PAWN, STEP, "Bishop pawn"));
            // tunings.add(new Tuning(EvalConstants.SPACE, STEP, "Space"));
            //
            // /* pawns */
            // tunings.add(new Tuning(EvalConstants.PASSED_SCORE_EG, STEP, "Passed score eg", 0));
            // tunings.add(new MultiTuning(EvalConstants.PASSED_MULTIPLIERS, "Passed multi"));
            // tunings.add(new MultiTuning(EvalConstants.PASSED_KING_MULTI, "Passed king multi"));
            // tunings.add(new Tuning(EvalConstants.PASSED_CANDIDATE, STEP, "Passed candidate", 0));
            // tunings.add(new TableTuning(EvalConstants.SHIELD_BONUS_MG, STEP, "Shield mg"));
            // tunings.add(new TableTuning(EvalConstants.SHIELD_BONUS_EG, STEP, "Shield eg"));
            // tunings.add(new Tuning(EvalConstants.PAWN_BLOCKAGE, STEP, "Pawn blockage", 0, 1));
            // tunings.add(new Tuning(EvalConstants.PAWN_CONNECTED, STEP, "Pawn connected", 0, 1));
            // tunings.add(new Tuning(EvalConstants.PAWN_NEIGHBOUR, STEP, "Pawn neighbour", 0, 1));
            //
            // /* king-safety */
            // tunings.add(new Tuning(EvalConstants.KS_SCORES, 10, "KS"));
            // tunings.add(new Tuning(EvalConstants.KS_QUEEN_TROPISM, 1, "KS queen", 0, 1));
            // tunings.add(new Tuning(EvalConstants.KS_CHECK_QUEEN, 1, "KS check q", 0, 1, 2, 3));
            // tunings.add(new Tuning(EvalConstants.KS_FRIENDS, 1, "KS friends"));
            // tunings.add(new Tuning(EvalConstants.KS_WEAK, 1, "KS weak"));
            // tunings.add(new Tuning(EvalConstants.KS_ATTACKS, 1, "KS attacks"));
            // tunings.add(new Tuning(EvalConstants.KS_NIGHT_DEFENDERS, 1, "KS night defenders"));
            // tunings.add(new Tuning(EvalConstants.KS_DOUBLE_ATTACKS, 1, "KS double attacks"));
            // tunings.add(new Tuning(EvalConstants.KS_ATTACK_PATTERN, 1, "KS pattern"));
            // tunings.add(new Tuning(EvalConstants.KS_OTHER, 1, "KS other"));
            //
            // /* mobility */
            // tunings.add(new Tuning(EvalConstants.MOBILITY_KNIGHT_MG, STEP, "Mobility n mg", true));
            // tunings.add(new Tuning(EvalConstants.MOBILITY_KNIGHT_EG, STEP, "Mobility n eg", true));
            // tunings.add(new Tuning(EvalConstants.MOBILITY_BISHOP_MG, STEP, "Mobility b mg", true));
            // tunings.add(new Tuning(EvalConstants.MOBILITY_BISHOP_EG, STEP, "Mobility b eg", true));
            // tunings.add(new Tuning(EvalConstants.MOBILITY_ROOK_MG, STEP, "Mobility r mg", true));
            // tunings.add(new Tuning(EvalConstants.MOBILITY_ROOK_EG, STEP, "Mobility r eg", true));
            // tunings.add(new Tuning(EvalConstants.MOBILITY_QUEEN_MG, STEP, "Mobility q mg", true));
            // tunings.add(new Tuning(EvalConstants.MOBILITY_QUEEN_EG, STEP, "Mobility q eg", true));
            // tunings.add(new Tuning(EvalConstants.MOBILITY_KING_MG, STEP, "Mobility k mg", true));
            // tunings.add(new Tuning(EvalConstants.MOBILITY_KING_EG, STEP, "Mobility k eg", true));
            //
            // /* psqt */
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_MG[ChessConstants.PAWN], STEP, "PSQT p mg", true));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_EG[ChessConstants.PAWN], STEP, "PSQT p eg", true));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_MG[ChessConstants.NIGHT], STEP, "PSQT n mg"));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_EG[ChessConstants.NIGHT], STEP, "PSQT n eg"));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_MG[ChessConstants.BISHOP], STEP, "PSQT b mg"));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_EG[ChessConstants.BISHOP], STEP, "PSQT b eg"));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_MG[ChessConstants.ROOK], STEP, "PSQT r mg"));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_EG[ChessConstants.ROOK], STEP, "PSQT r eg"));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_MG[ChessConstants.QUEEN], STEP, "PSQT q mg"));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_EG[ChessConstants.QUEEN], STEP, "PSQT q eg"));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_MG[ChessConstants.KING], STEP, "PSQT k mg"));
            // tunings.add(new PsqtTuning(EvalConstants.PSQT_EG[ChessConstants.KING], STEP, "PSQT k eg"));

            return tunings;
        }

        public static async Task Main()
        {
            // read all fens, including score
            var fens = LoadFens("d:\\backup\\chess\\epds\\quiet-labeled.epd", true, false);
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
            var tuningObjects = GetTuningObjects();

            // tune
            await PrintInfo(tuningObjects);
            await LocalOptimize(tuningObjects);
            Console.WriteLine($"\nDone: {_orgError} -> {_bestError}\n");
        }

        private static void PrintAll(IEnumerable<Tuning> tuningObjects)
        {
            foreach (var tuningObject in tuningObjects)
                if (tuningObject.IsUpdated())
                    tuningObject.PrintNewValues();
                else
                    Console.WriteLine(tuningObject.Name + ": unchanged");
        }

        public static Dictionary<string, double> LoadFens(string fileName, bool containsResult, bool includingCheck)
        {
            Console.WriteLine("Loading " + fileName);

            var fens = new Dictionary<string, double>();
            var checkCount = 0;
            var checkmate = 0;
            var stalemate = 0;

            try
            {
                foreach (var line in File.ReadLines(fileName))
                {
                    double score = 0;
                    string fenString;
                    if (containsResult)
                    {
                        var scoreString = GetScoreStringFromLine(line);
                        fenString = GetFenStringFromLine(line);
                        if (scoreString.Equals("\"1/2-1/2\";"))
                            score = 0.5;
                        else if (scoreString.Equals("\"1-0\";"))
                            score = 1;
                        else if (scoreString.Equals("\"0-1\";"))
                            score = 0;
                        else
                            throw new Exception("Unknown result: " + scoreString);
                    }
                    else
                    {
                        fenString = line;
                    }

                    ChessBoardUtil.SetFen(fenString, Cb);

                    if (Cb.CheckingPieces == 0)
                    {
                        ThreadData.StartPly();
                        MoveGenerator.GenerateAttacks(ThreadData, Cb);
                        MoveGenerator.GenerateMoves(ThreadData, Cb);
                        if (ThreadData.HasNext())
                            fens.Add(fenString, score);
                        else
                            stalemate++;

                        ThreadData.EndPly();
                    }
                    else
                    {
                        checkCount++;
                        if (!includingCheck) continue;
                        ThreadData.StartPly();
                        MoveGenerator.GenerateAttacks(ThreadData, Cb);
                        MoveGenerator.GenerateMoves(ThreadData, Cb);
                        if (ThreadData.HasNext())
                            fens.Add(fenString, score);
                        else
                            checkmate++;

                        ThreadData.EndPly();
                    }

                    //line = br.readLine();
                }
            }
            catch (IOException e)
            {
                throw new Exception("", e);
            }

            Console.WriteLine("In check : " + checkCount);
            Console.WriteLine("Checkmate : " + checkmate);
            Console.WriteLine("Stalemate : " + stalemate);
            return fens;
        }

        private static string GetFenStringFromLine(string line)
        {
            return line.Contains("c9")
                ? line.Split(" c9 ")[0]
                : line.Substring(0, line.IndexOf("\"", StringComparison.Ordinal));
        }

        private static string GetScoreStringFromLine(string line)
        {
            return line.Contains("c9")
                ? line.Split(" c9 ")[1]
                : line.Substring(line.IndexOf("\"", StringComparison.Ordinal));
        }

        private static async Task PrintInfo(IEnumerable<Tuning> tuningObjects)
        {
            TimeUtil.Reset();
            Console.WriteLine("\nNumber of threads: " + NumberOfThreads);
            Console.WriteLine("\nValues that are being tuned:");

            var totalValues = 0;
            foreach (var tuningObject in tuningObjects)
            {
                if (tuningObject.ShowAverage)
                    Console.WriteLine(tuningObject.Name + " " + tuningObject.GetAverage());
                else
                    Console.WriteLine(tuningObject.Name);

                totalValues += tuningObject.GetNumberOfTunedValues();
            }

            Console.WriteLine(
                $"\nInitial error: {await CalculateErrorMultiThreaded()} ({TimeUtil.GetPassedTimeMs()} ms)");
            Console.WriteLine("Total values to be tuned: " + totalValues + "\n");
        }

        private static async Task LocalOptimize(IReadOnlyCollection<Tuning> tuningObjects)
        {
            var bestErrorLocal = await CalculateErrorMultiThreaded();
            _orgError = bestErrorLocal;
            var improved = true;
            var run = 1;
            while (improved)
            {
                Console.WriteLine("Run " + run++);
                improved = false;
                foreach (var tuningObject in tuningObjects)
                    for (var i = 0; i < tuningObject.NumberOfParameters(); i++)
                    {
                        if (tuningObject.Skip(i)) continue;

                        tuningObject.AddStep(i);
                        EvalConstants.InitMgEg();
                        ThreadData.ClearCaches();
                        var newError = await CalculateErrorMultiThreaded();
                        if (newError < bestErrorLocal - 0.00000001)
                        {
                            bestErrorLocal = newError;
                            Console.WriteLine($"{bestErrorLocal} - {tuningObject}");
                            improved = true;
                        }
                        else
                        {
                            tuningObject.RemoveStep(i);
                            tuningObject.RemoveStep(i);
                            EvalConstants.InitMgEg();
                            ThreadData.ClearCaches();
                            newError = await CalculateErrorMultiThreaded();
                            if (newError < bestErrorLocal - 0.00000001)
                            {
                                bestErrorLocal = newError;
                                Console.WriteLine($"{bestErrorLocal} - {tuningObject}");
                                improved = true;
                            }
                            else
                            {
                                tuningObject.AddStep(i);
                            }
                        }
                    }

                PrintAll(tuningObjects);
            }

            _bestError = bestErrorLocal;
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
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine(e);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            return totalError / NumberOfThreads;
        }
    }
}