using System;
using System.Collections.Generic;
using Chess22kDotNet.Texel;
using static Chess22kDotNet.ChessConstants;

namespace Chess22kDotNet.Eval
{
    /**
    * Values have been tuned using the Texel's tuning method
    */
    public static class EvalConstants
    {
        public const int SideToMoveBonus = 16; //cannot be tuned //TODO lower in endgame

        public const int ScoreDraw = 0;
        public const int ScoreMateBound = 30000;

        // other
        public static readonly int[] OtherScores = {-10, 14, 18, -8, 16, 12, -158, 12, 492, 24, -44, 26};
        public const int IxRookFileSemiOpen = 0;
        public const int IxRookFileSemiOpenIsolated = 1;
        public const int IxRookFileOpen = 2;
        public const int IxRook7ThRank = 3;
        public const int IxRookBattery = 4;
        public const int IxBishopLong = 5;
        public const int IxBishopPrison = 6;
        public const int IxSpace = 7;
        public const int IxDrawish = 8;
        public const int IxCastling = 9;
        public const int IxRookTrapped = 10;
        public const int IxOutpost = 11;

        // threats
        public static readonly int[] ThreatsMg = {38, 66, 90, 16, 66, 38, 12, 16, -6};
        public static readonly int[] ThreatsEg = {34, 20, -64, 16, 10, -48, 28, 4, 14};
        public static readonly int[] Threats = new int[ThreatsMg.Length];
        public const int IxMultiplePawnAttacks = 0;
        public const int IxPawnAttacks = 1;
        public const int IxQueenAttacked = 2;
        public const int IxPawnPushThreat = 3;
        public const int IxRookAttacked = 4;
        public const int IxQueenAttackedMinor = 5;
        public const int IxMajorAttacked = 6;
        public const int IxUnusedOutpost = 7;
        public const int IxPawnAttacked = 8;

        // pawn
        public static readonly int[] PawnScores = {10, 10, 12, 6};
        public const int IxPawnDouble = 0;
        public const int IxPawnIsolated = 1;
        public const int IxPawnBackward = 2;
        public const int IxPawnInverse = 3;

        // imbalance
        public static readonly int[] ImbalanceScores = {-10, 50, 12};
        public const int IxRookPair = 0;
        public const int IxBishopDouble = 1;
        public const int IxQueenNight = 2;

        public static readonly int[] Phase = {0, 0, 9, 10, 20, 40};

        public static readonly int[] Material = {0, 100, 398, 438, 710, 1380, 3000};
        public static readonly int[] KnightPawn = {42, -16, 0, 4, 10, 12, 20, 30, 36};
        public static readonly int[] RookPawn = {50, -2, -4, -2, -4, 0, 0, 0, 0};
        public static readonly int[] BishopPawn = {20, 8, 6, 0, -6, -12, -18, -28, -34};

        public static readonly int[] Pinned = {0, 6, -14, -52, -68, -88};
        public static readonly int[] Discovered = {0, -14, 124, 98, 176, 0, 32};
        public static readonly int[] DoubleAttacked = {0, 16, 34, 72, 4, -14, 0};
        public static readonly int[] Space = {0, 0, 124, 0, 0, -6, -6, -8, -7, -4, -4, -2, 0, -1, 0, 3, 7};

        public static readonly int[] PawnBlockage = {0, 0, -8, 2, 6, 32, 66, 192};
        public static readonly int[] PawnConnected = {0, 0, 14, 16, 24, 62, 138};
        public static readonly int[] PawnNeighbour = {0, 0, 4, 12, 28, 92, 326};

        private static readonly int[][] ShieldBonusMg =
        {
            new[] {0, 22, 22, -8, -12, 22, -258},
            new[] {0, 48, 40, -6, -6, 154, -234},
            new[] {0, 48, 0, -14, 54, 148, 16},
            new[] {0, 8, 0, -6, -20, 138, 34}
        };

        private static readonly int[][] ShieldBonusEg =
        {
            new[] {0, -56, -22, -8, 28, 2, -52},
            new[] {0, -16, -22, 6, 54, 38, 52},
            new[] {0, 0, 20, 20, 28, 80, 40},
            new[] {0, -26, -10, 20, 48, 46, 180}
        };

        public static readonly int[][] ShieldBonus = Util.CreateJaggedArray<int[][]>(4, 7);

        public static readonly int[] PassedScoreEg = {0, 14, 16, 34, 62, 128, 232};
        public static readonly int[] PassedCandidate = {0, 0, 0, 8, 14, 42};
        public static readonly float[] PassedKingMulti = {0, 1.4f, 1.4f, 1.2f, 1.1f, 1.0f, 0.8f, 0.8f};

        public static readonly float[] PassedMultipliers =
        {
            0.5f, // blocked
            1.3f, // next square attacked
            0.4f, // enemy king in front
            1.2f, // next square defended
            0.7f, // attacked
            1.7f, // defended by rook from behind
            0.6f, // attacked by rook from behind
            1.8f // no enemy attacks in front
        };

        //concept borrowed from Ed Schroder
        public static readonly int[] KsScores =
        {
            0, 0, 0, 40, 60, 70, 80, 90, 100, 120, 150, 200, 260, 300, 390, 450, 520, 640, 740, 760, 1260
        };

        public static readonly int[] KsQueenTropism = {0, 0, 2, 2, 2, 2, 1, 1}; // index 0 and 1 are never evaluated	
        public static readonly int[] KsCheckQueen = {0, 0, 0, 0, 1, 2, 3, 3, 3, 3, 3, 3, 2, 1, 0, 0, 0};
        public static readonly int[] KsFriends = {2, 2, 1, 1, 0, 0, 0, 0, 3};
        public static readonly int[] KsWeak = {0, 1, 2, 2, 2, 2, 2, 1, -5};
        public static readonly int[] KsAttacks = {0, 2, 2, 2, 2, 2, 3, 4, 4};
        public static readonly int[] KsKnightDefenders = {1, 0, 0, 0, 0, 0, 0, 0, 0};
        public static readonly int[] KsDoubleAttacks = {0, 1, 1, 3, 3, 9, 0, 0, 0};

        public static readonly int[] KsAttackPattern =
        {
            //                                                 Q  Q  Q  Q  Q  Q  Q  Q  Q  Q  Q  Q  Q  Q  Q  Q  
            // 	                    R  R  R  R  R  R  R  R                          R  R  R  R  R  R  R  R  
            //             B  B  B  B              B  B  B  B              B  B  B  B              B  B  B  B  
            //       N  N        N  N        N  N        N  N        N  N        N  N        N  N        N  N  
            //    P     P     P     P     P     P     P     P     P     P     P     P     P     P     P     P
            4, 1, 2, 2, 2, 2, 2, 3, 1, 0, 1, 2, 1, 1, 1, 2, 2, 2, 2, 3, 2, 2, 3, 4, 1, 2, 3, 3, 2, 3, 3, 5
        };

        public static readonly int[] KsOther =
        {
            2, // queen-touch check
            3, // king blocked at first rank check
            3, // safe check
            1 // unsafe check
        };

        private static readonly int[] MobilityKnightMg = {-36, -16, -6, 2, 12, 16, 24, 24, 42};
        private static readonly int[] MobilityKnightEg = {-98, -30, -12, 0, 4, 16, 16, 18, 8};
        private static readonly int[] MobilityBishopMg = {-32, -16, -4, 4, 8, 14, 16, 16, 14, 16, 30, 38, -14, 54};
        private static readonly int[] MobilityBishopEg = {-54, -28, -10, 0, 8, 12, 16, 18, 22, 20, 14, 18, 38, 18};

        private static readonly int[] MobilityRookMg =
            {-54, -44, -40, -34, -32, -24, -20, -8, 0, 10, 14, 24, 32, 40, 26};

        private static readonly int[] MobilityRookEg = {-62, -38, -22, -12, 2, 4, 12, 8, 14, 14, 18, 20, 20, 22, 30};

        private static readonly int[] MobilityQueenMg =
        {
            -10, -14, -8, -14, -8, -6, -10, -8, -6, -4, -2, 2, 0, 6, 2, 6, 0, 14, 10, 16, 32, 66, 6, 150, 152, 236, 72,
            344
        };

        private static readonly int[] MobilityQueenEg =
        {
            -78, -100, -102, -78, -84, -54, -24, -16, -6, 6, 16, 20, 26, 32, 40, 46, 56, 46, 62, 66, 60, 56, 72, 18, 4,
            -24, 64, -90
        };

        private static readonly int[] MobilityKingMg = {-4, -2, 0, 4, 10, 18, 24, 46, 62};
        private static readonly int[] MobilityKingEg = {-22, 4, 12, 8, 2, -14, -16, -30, -64};
        public static readonly int[] MobilityKnight = new int[MobilityKnightMg.Length];
        public static readonly int[] MobilityBishop = new int[MobilityBishopMg.Length];
        public static readonly int[] MobilityRook = new int[MobilityRookMg.Length];
        public static readonly int[] MobilityQueen = new int[MobilityQueenMg.Length];
        public static readonly int[] MobilityKing = new int[MobilityKingMg.Length];

        /** piece, color, square */
        public static readonly int[][][] Psqt = Util.CreateJaggedArray<int[][][]>(7, 2, 64);

        private static readonly int[][][] PsqtMg = Util.CreateJaggedArray<int[][][]>(7, 2, 64);
        private static readonly int[][][] PsqtEg = Util.CreateJaggedArray<int[][][]>(7, 2, 64);

        public static readonly int[] MirroredLeftRight = new int[64];
        public static readonly int[] MirroredUpDown = new int[64];

        static EvalConstants()
        {
            PsqtMg[Pawn][White] = new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0,
                146, 150, 174, 216, 216, 174, 150, 146,
                10, 20, 62, 54, 54, 62, 20, 10,
                -18, -10, -8, 10, 10, -8, -10, -18,
                -32, -30, -12, 2, 2, -12, -30, -32,
                -30, -22, -14, -16, -16, -14, -22, -30,
                -24, 2, -18, -10, -10, -18, 2, -24,
                0, 0, 0, 0, 0, 0, 0, 0
            };

            PsqtEg[Pawn][White] = new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0,
                -24, -22, -34, -46, -46, -34, -22, -24,
                32, 20, -4, -18, -18, -4, 20, 32,
                26, 14, 10, -4, -4, 10, 14, 26,
                16, 12, 6, -2, -2, 6, 12, 16,
                8, 6, 4, 14, 14, 4, 6, 8,
                18, 6, 18, 24, 24, 18, 6, 18,
                0, 0, 0, 0, 0, 0, 0, 0
            };

            PsqtMg[Knight][White] = new[]
            {
                -218, -114, -132, -34, -34, -132, -114, -218,
                -78, -60, 10, -30, -30, 10, -60, -78,
                -12, 48, 40, 56, 56, 40, 48, -12,
                14, 50, 46, 58, 58, 46, 50, 14,
                12, 40, 44, 42, 42, 44, 40, 12,
                6, 40, 40, 38, 38, 40, 40, 6,
                -8, 6, 18, 32, 32, 18, 6, -8,
                -30, 2, -6, 12, 12, -6, 2, -30
            };

            PsqtEg[Knight][White] = new[]
            {
                -16, 6, 36, 18, 18, 36, 6, -16,
                4, 36, 12, 42, 42, 12, 36, 4,
                -6, 10, 32, 30, 30, 32, 10, -6,
                18, 18, 38, 40, 40, 38, 18, 18,
                14, 18, 32, 40, 40, 32, 18, 14,
                8, 4, 12, 30, 30, 12, 4, 8,
                -12, 8, 10, 14, 14, 10, 8, -12,
                -10, -6, 8, 16, 16, 8, -6, -10
            };

            PsqtMg[Bishop][White] = new[]
            {
                -26, 4, -92, -88, -88, -92, 4, -26,
                -52, -14, -2, -38, -38, -2, -14, -52,
                36, 48, 40, 30, 30, 40, 48, 36,
                18, 34, 40, 54, 54, 40, 34, 18,
                32, 40, 44, 60, 60, 44, 40, 32,
                36, 52, 52, 42, 42, 52, 52, 36,
                34, 62, 48, 44, 44, 48, 62, 34,
                8, 36, 30, 50, 50, 30, 36, 8
            };

            PsqtEg[Bishop][White] = new[]
            {
                -30, -10, -2, 6, 6, -2, -10, -30,
                0, -10, 4, 6, 6, 4, -10, 0,
                -10, -10, -10, -4, -4, -10, -10, -10,
                -4, -6, 0, 4, 4, 0, -6, -4,
                -20, -12, -6, -4, -4, -6, -12, -20,
                -20, -16, -18, 0, 0, -18, -16, -20,
                -30, -40, -22, -10, -10, -22, -40, -30,
                -30, -18, -8, -18, -18, -8, -18, -30
            };

            PsqtMg[Rook][White] = new[]
            {
                -48, -14, -76, -4, -4, -76, -14, -48,
                -20, -16, 18, 42, 42, 18, -16, -20,
                -28, 0, -8, -4, -4, -8, 0, -28,
                -40, -18, 6, 6, 6, 6, -18, -40,
                -40, -10, -14, 6, 6, -14, -10, -40,
                -38, -16, -6, -2, -2, -6, -16, -38,
                -50, -10, -10, 8, 8, -10, -10, -50,
                -26, -14, -4, 10, 10, -4, -14, -26
            };

            PsqtEg[Rook][White] = new[]
            {
                54, 48, 68, 46, 46, 68, 48, 54,
                40, 44, 32, 20, 20, 32, 44, 40,
                36, 36, 34, 30, 30, 34, 36, 36,
                42, 38, 40, 30, 30, 40, 38, 42,
                34, 32, 32, 24, 24, 32, 32, 34,
                22, 26, 16, 14, 14, 16, 26, 22,
                22, 10, 14, 10, 10, 14, 10, 22,
                14, 16, 14, 6, 6, 14, 16, 14
            };

            PsqtMg[Queen][White] = new[]
            {
                -52, -44, -60, -54, -54, -60, -44, -52,
                -38, -80, -52, -74, -74, -52, -80, -38,
                0, -18, -40, -60, -60, -40, -18, 0,
                -30, -40, -36, -48, -48, -36, -40, -30,
                -24, -22, -14, -18, -18, -14, -22, -24,
                -6, 8, -14, -4, -4, -14, 8, -6,
                -8, 10, 26, 20, 20, 26, 10, -8,
                6, 2, 8, 20, 20, 8, 2, 6
            };

            PsqtEg[Queen][White] = new[]
            {
                20, 18, 34, 28, 28, 34, 18, 20,
                6, 22, 8, 48, 48, 8, 22, 6,
                -12, -2, 8, 34, 34, 8, -2, -12,
                32, 40, 16, 26, 26, 16, 40, 32,
                18, 26, 0, 14, 14, 0, 26, 18,
                8, -28, 8, -4, -4, 8, -28, 8,
                -14, -32, -28, -6, -6, -28, -32, -14,
                -24, -24, -18, -10, -10, -18, -24, -24
            };

            PsqtMg[King][White] = new[]
            {
                -16, 204, -18, 16, 16, -18, 204, -16,
                38, -2, -60, -20, -20, -60, -2, 38,
                36, 60, 56, -20, -20, 56, 60, 36,
                -26, -8, -46, -80, -80, -46, -8, -26,
                -64, -24, -40, -72, -72, -40, -24, -64,
                -6, 8, -6, -18, -18, -6, 8, -6,
                32, 26, -22, -28, -28, -22, 26, 32,
                28, 44, 12, -2, -2, 12, 44, 28
            };

            PsqtEg[King][White] = new[]
            {
                -104, -82, 14, -52, -52, 14, -82, -104,
                -28, 22, 50, 28, 28, 50, 22, -28,
                -4, 36, 36, 34, 34, 36, 36, -4,
                -4, 36, 44, 46, 46, 44, 36, -4,
                -12, 10, 28, 40, 40, 28, 10, -12,
                -18, 10, 18, 24, 24, 18, 10, -18,
                -38, 0, 20, 24, 24, 20, 0, -38,
                -72, -40, -24, -30, -30, -24, -40, -72
            };

            for (var i = 0; i < 64; i++)
            {
                MirroredLeftRight[i] = i / 8 * 8 + 7 - (i & 7);
            }

            for (var i = 0; i < 64; i++)
            {
                MirroredUpDown[i] = (7 - i / 8) * 8 + (i & 7);
            }

            // fix white arrays
            for (var piece = Pawn; piece <= King; piece++)
            {
                Util.Reverse(PsqtMg[piece][White]);
                Util.Reverse(PsqtEg[piece][White]);
            }

            // create black arrays
            for (var piece = Pawn; piece <= King; piece++)
            {
                for (var i = 0; i < 64; i++)
                {
                    PsqtMg[piece][Black][i] = -PsqtMg[piece][White][MirroredUpDown[i]];
                    PsqtEg[piece][Black][i] = -PsqtEg[piece][White][MirroredUpDown[i]];
                }
            }

            Util.Reverse(RookPrison);
            Util.Reverse(BishopPrison);

            InitMgEg();
        }

        public static readonly long[] RookPrison =
        {
            0, Bitboard.A8, Bitboard.A8B8, Bitboard.A8B8C8, 0, Bitboard.G8H8, Bitboard.H8, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, Bitboard.A1, Bitboard.A1B1, Bitboard.A1B1C1, 0, Bitboard.G1H1, Bitboard.H1, 0
        };

        public static readonly long[] BishopPrison =
        {
            0, 0, 0, 0, 0, 0, 0, 0, //8
            Bitboard.B6C7, 0, 0, 0, 0, 0, 0, Bitboard.G6F7, //7
            0, 0, 0, 0, 0, 0, 0, 0, //6
            0, 0, 0, 0, 0, 0, 0, 0, //5
            0, 0, 0, 0, 0, 0, 0, 0, //4
            0, 0, 0, 0, 0, 0, 0, 0, //3
            Bitboard.B3C2, 0, 0, 0, 0, 0, 0, Bitboard.G3F2, //2
            0, 0, 0, 0, 0, 0, 0, 0 //1
            // A  B  C  D  E  F  G  H
        };

        public static readonly int[] PromotionScore =
        {
            0,
            0,
            Material[Knight] - Material[Pawn],
            Material[Bishop] - Material[Pawn],
            Material[Rook] - Material[Pawn],
            Material[Queen] - Material[Pawn],
        };


        public static void InitMgEg()
        {
            InitMgEg(MobilityKnight, MobilityKnightMg, MobilityKnightEg);
            InitMgEg(MobilityBishop, MobilityBishopMg, MobilityBishopEg);
            InitMgEg(MobilityRook, MobilityRookMg, MobilityRookEg);
            InitMgEg(MobilityQueen, MobilityQueenMg, MobilityQueenEg);
            InitMgEg(MobilityKing, MobilityKingMg, MobilityKingEg);
            InitMgEg(Threats, ThreatsMg, ThreatsEg);

            for (var i = 0; i < 4; i++)
            {
                InitMgEg(ShieldBonus[i], ShieldBonusMg[i], ShieldBonusEg[i]);
            }

            for (var color = White; color <= Black; color++)
            {
                for (var piece = Pawn; piece <= King; piece++)
                {
                    InitMgEg(Psqt[piece][color], PsqtMg[piece][color], PsqtEg[piece][color]);
                }
            }
        }

        private static void InitMgEg(IList<int> array, IReadOnlyList<int> arrayMg, IReadOnlyList<int> arrayEg)
        {
            for (var i = 0; i < array.Count; i++)
            {
                array[i] = EvalUtil.Score(arrayMg[i], arrayEg[i]);
            }
        }

        public static void Main()
        {
            //increment a psqt with a constant
            for (var i = 0; i < 64; i++)
            {
                PsqtEg[King][White][i] += 20;
            }

            Console.WriteLine(PsqtTuning.GetArrayFriendlyFormatted(PsqtEg[King][White]));
        }
    }
}