using System.Numerics;

namespace Chess22kDotNet.Move
{
    public static class MagicUtil
    {
        // rook-size: 800kb
        // bishop-size: 40kb

        // TODO smaller tables?
        private static readonly long[] RookMagicNumbers =
        {
            -0x5E7FFDDF7FBFFDD0L, 0x40100040022000L, 0x80088020001002L, 0x80080280841000L, 0x4200042010460008L,
            0x4800a0003040080L, 0x400110082041008L, 0x8000a041000880L, 0x10138001a080c010L, 0x804008200480L,
            0x10011012000c0L, 0x22004128102200L,
            0x200081201200cL, 0x202a001048460004L, 0x81000100420004L, 0x4000800380004500L, 0x208002904001L,
            0x90004040026008L, 0x208808010002001L,
            0x2002020020704940L, -0x7FB7FEFFF7EEFFFBL, 0x6820808004002200L, 0xa80040008023011L, 0xb1460000811044L,
            0x4204400080008ea0L, -0x4FFDBFFE7FDFFE7CL,
            0x2020200080100380L, 0x10080080100080L, 0x2204080080800400L, 0xa40080360080L, 0x2040604002810b1L,
            0x8c218600004104L, -0x7E7FFFBFFFBFE000L,
            0x488c402000401001L, 0x4018a00080801004L, 0x1230002105001008L, -0x76FB7FF7FF7FFC00L, 0x42000c42003810L,
            0x8408110400b012L, 0x18086182000401L,
            0x2240088020c28000L, 0x1001201040c004L, 0xa02008010420020L, 0x10003009010060L, 0x4008008008014L,
            0x80020004008080L, 0x282020001008080L,
            0x50000181204a0004L, 0x102042111804200L, 0x40002010004001c0L, 0x19220045508200L, 0x20030010060a900L,
            0x8018028040080L, 0x88240002008080L,
            0x10301802830400L, 0x332a4081140200L, 0x8080010a601241L, 0x1008010400021L, 0x4082001007241L,
            0x211009001200509L, -0x7FEAFFEFFDBBE7FFL,
            0x801000804000603L, 0xc0900220024a401L, 0x1000200608243L
        };

        private static readonly long[] BishopMagicNumbers =
        {
            0x2910054208004104L, 0x2100630a7020180L, 0x5822022042000000L, 0x2ca804a100200020L, 0x204042200000900L,
            0x2002121024000002L, -0x7FBFBEFBDFDFFF18L, -0x7ED5FDFDFAFEF7C0L, -0x7FFAE7EE7BF7FFB8L,
            0x1001c20208010101L, 0x1001080204002100L, 0x1810080489021800L,
            0x62040420010a00L, 0x5028043004300020L, -0x3FF7F5BBFD9FAFFEL, 0x8a00a0104220200L, 0x940000410821212L,
            0x1808024a280210L, 0x40c0422080a0598L,
            0x4228020082004050L, 0x200800400e00100L, 0x20b001230021040L, 0x90a0201900c00L, 0x4940120a0a0108L,
            0x20208050a42180L, 0x1004804b280200L,
            0x2048020024040010L, 0x102c04004010200L, 0x20408204c002010L, 0x2411100020080c1L, 0x102a008084042100L,
            0x941030000a09846L, 0x244100800400200L,
            0x4000901010080696L, 0x280404180020L, 0x800042008240100L, 0x220008400088020L, 0x4020182000904c9L,
            0x23010400020600L, 0x41040020110302L,
            0x412101004020818L, -0x7FDDF7F5F6BFBDF8L, 0x1401210240484800L, 0x22244208010080L, 0x1105040104000210L,
            0x2040088800c40081L, -0x7E7B7EFDADFFFC00L,
            0x4004610041002200L, 0x40201a444400810L, 0x4611010802020008L, -0x7FFFF4FBFEFBFBFEL, 0x20004821880a00L,
            -0x7DFFFFDFDDBBFF00L, 0x9431801010068L,
            0x1040c20806108040L, 0x804901403022a40L, 0x2400202602104000L, 0x208520209440204L, 0x40c000022013020L,
            0x2000104000420600L, 0x400000260142410L,
            0x800633408100500L, 0x2404080a1410L, 0x138200122002900L
        };

        private static readonly Magic[] RookMagics = new Magic[0x40];
        private static readonly Magic[] BishopMagics = new Magic[0x40];

        public static long GetRookMoves(in int fromIndex, in long allPieces)
        {
            var magic = RookMagics[fromIndex];
            return magic.MagicMoves[
                (int) Util.RightTripleShift((allPieces & magic.MovementMask) * magic.MagicNumber, magic.Shift)];
        }

        public static long GetBishopMoves(in int fromIndex, in long allPieces)
        {
            var magic = BishopMagics[fromIndex];
            return magic.MagicMoves[
                (int) Util.RightTripleShift((allPieces & magic.MovementMask) * magic.MagicNumber, magic.Shift)];
        }

        public static long GetQueenMoves(in int fromIndex, in long allPieces)
        {
            var rookMagic = RookMagics[fromIndex];
            var bishopMagic = BishopMagics[fromIndex];
            return rookMagic.MagicMoves[
                       (int) Util.RightTripleShift((allPieces & rookMagic.MovementMask) * rookMagic.MagicNumber,
                           rookMagic.Shift)]
                   | bishopMagic.MagicMoves[
                       (int) Util.RightTripleShift((allPieces & bishopMagic.MovementMask) * bishopMagic.MagicNumber,
                           bishopMagic.Shift)];
        }

        public static long GetRookMovesEmptyBoard(in int fromIndex)
        {
            return RookMagics[fromIndex].MagicMoves[0x0];
        }

        public static long GetBishopMovesEmptyBoard(in int fromIndex)
        {
            return BishopMagics[fromIndex].MagicMoves[0x0];
        }

        public static long GetQueenMovesEmptyBoard(in int fromIndex)
        {
            return BishopMagics[fromIndex].MagicMoves[0x0] | RookMagics[fromIndex].MagicMoves[0x0];
        }

        static MagicUtil()
        {
            for (var i = 0x0; i < 0x40; i++)
            {
                RookMagics[i] = new Magic(RookMagicNumbers[i]);
                BishopMagics[i] = new Magic(BishopMagicNumbers[i]);
            }

            CalculateBishopMovementMasks();
            CalculateRookMovementMasks();
            GenerateShiftArrys();
            var bishopOccupancyVariations = CalculateVariations(BishopMagics);
            var rookOccupancyVariations = CalculateVariations(RookMagics);
            GenerateBishopMoveDatabase(bishopOccupancyVariations);
            GenerateRookMoveDatabase(rookOccupancyVariations);
        }

        private static void GenerateShiftArrys()
        {
            for (var i = 0x0; i < 0x40; i++)
            {
                RookMagics[i].Shift = 0x40 - BitOperations.PopCount((ulong) RookMagics[i].MovementMask);
                BishopMagics[i].Shift = 0x40 - BitOperations.PopCount((ulong) BishopMagics[i].MovementMask);
            }
        }

        private static long[][] CalculateVariations(Magic[] magics)
        {
            var occupancyVariations = new long[0x40][];
            for (var index = 0x0; index < 0x40; index++)
            {
                var variationCount = (int) Util.PowerLookup[BitOperations.PopCount((ulong) magics[index].MovementMask)];
                occupancyVariations[index] = new long[variationCount];

                for (var variationIndex = 0x1; variationIndex < variationCount; variationIndex++)
                {
                    var currentMask = magics[index].MovementMask;

                    for (var i = 0x0; i < 0x20 - BitOperations.LeadingZeroCount((uint) variationIndex); i++)
                    {
                        if ((Util.PowerLookup[i] & variationIndex) != 0x0)
                        {
                            occupancyVariations[index][variationIndex] |= currentMask & -currentMask;
                        }

                        currentMask &= currentMask - 0x1;
                    }
                }
            }

            return occupancyVariations;
        }

        private static void CalculateRookMovementMasks()
        {
            for (var index = 0x0; index < 0x40; index++)
            {
                // up
                for (var j = index + 0x8; j < 0x40 - 0x8; j += 0x8)
                {
                    RookMagics[index].MovementMask |= Util.PowerLookup[j];
                }

                // down
                for (var j = index - 0x8; j >= 0x0 + 0x8; j -= 0x8)
                {
                    RookMagics[index].MovementMask |= Util.PowerLookup[j];
                }

                // left
                for (var j = index + 0x1; j % 0x8 != 0x0 && j % 0x8 != 0x7; j++)
                {
                    RookMagics[index].MovementMask |= Util.PowerLookup[j];
                }

                // right
                for (var j = index - 0x1; j % 0x8 != 0x7 && j % 0x8 != 0x0 && j > 0x0; j--)
                {
                    RookMagics[index].MovementMask |= Util.PowerLookup[j];
                }
            }
        }

        private static void CalculateBishopMovementMasks()
        {
            for (var index = 0x0; index < 0x40; index++)
            {
                // up-right
                for (var j = index + 0x7; j < 0x40 - 0x7 && j % 0x8 != 0x7 && j % 0x8 != 0x0; j += 0x7)
                {
                    BishopMagics[index].MovementMask |= Util.PowerLookup[j];
                }

                // up-left
                for (var j = index + 0x9; j < 0x40 - 0x9 && j % 0x8 != 0x7 && j % 0x8 != 0x0; j += 0x9)
                {
                    BishopMagics[index].MovementMask |= Util.PowerLookup[j];
                }

                // down-right
                for (var j = index - 0x9; j >= 0x0 + 0x9 && j % 0x8 != 0x7 && j % 0x8 != 0x0; j -= 0x9)
                {
                    BishopMagics[index].MovementMask |= Util.PowerLookup[j];
                }

                // down-left
                for (var j = index - 0x7; j >= 0x0 + 0x7 && j % 0x8 != 0x7 && j % 0x8 != 0x0; j -= 0x7)
                {
                    BishopMagics[index].MovementMask |= Util.PowerLookup[j];
                }
            }
        }

        private static void GenerateRookMoveDatabase(long[][] rookOccupancyVariations)
        {
            for (var index = 0x0; index < 0x40; index++)
            {
                RookMagics[index].MagicMoves = new long[rookOccupancyVariations[index].Length];
                for (var variationIndex = 0x0; variationIndex < rookOccupancyVariations[index].Length; variationIndex++)
                {
                    long validMoves = 0x0;
                    var magicIndex = (int) Util.RightTripleShift(
                        rookOccupancyVariations[index][variationIndex] * RookMagicNumbers[index],
                        RookMagics[index].Shift);

                    for (var j = index + 0x8; j < 0x40; j += 0x8)
                    {
                        validMoves |= Util.PowerLookup[j];
                        if ((rookOccupancyVariations[index][variationIndex] & Util.PowerLookup[j]) != 0x0)
                        {
                            break;
                        }
                    }

                    for (var j = index - 0x8; j >= 0x0; j -= 0x8)
                    {
                        validMoves |= Util.PowerLookup[j];
                        if ((rookOccupancyVariations[index][variationIndex] & Util.PowerLookup[j]) != 0x0)
                        {
                            break;
                        }
                    }

                    for (var j = index + 0x1; j % 0x8 != 0x0; j++)
                    {
                        validMoves |= Util.PowerLookup[j];
                        if ((rookOccupancyVariations[index][variationIndex] & Util.PowerLookup[j]) != 0x0)
                        {
                            break;
                        }
                    }

                    for (var j = index - 0x1; j % 0x8 != 0x7 && j >= 0x0; j--)
                    {
                        validMoves |= Util.PowerLookup[j];
                        if ((rookOccupancyVariations[index][variationIndex] & Util.PowerLookup[j]) != 0x0)
                        {
                            break;
                        }
                    }

                    RookMagics[index].MagicMoves[magicIndex] = validMoves;
                }
            }
        }

        private static void GenerateBishopMoveDatabase(long[][] bishopOccupancyVariations)
        {
            for (var index = 0x0; index < 0x40; index++)
            {
                BishopMagics[index].MagicMoves = new long[bishopOccupancyVariations[index].Length];
                for (var variationIndex = 0x0;
                    variationIndex < bishopOccupancyVariations[index].Length;
                    variationIndex++)
                {
                    long validMoves = 0x0;
                    var magicIndex = (int) Util.RightTripleShift(
                        bishopOccupancyVariations[index][variationIndex] * BishopMagicNumbers[index],
                        BishopMagics[index].Shift);

                    // up-right
                    for (var j = index + 0x7; j % 0x8 != 0x7 && j < 0x40; j += 0x7)
                    {
                        validMoves |= Util.PowerLookup[j];
                        if ((bishopOccupancyVariations[index][variationIndex] & Util.PowerLookup[j]) != 0x0)
                        {
                            break;
                        }
                    }

                    // up-left
                    for (var j = index + 0x9; j % 0x8 != 0x0 && j < 0x40; j += 0x9)
                    {
                        validMoves |= Util.PowerLookup[j];
                        if ((bishopOccupancyVariations[index][variationIndex] & Util.PowerLookup[j]) != 0x0)
                        {
                            break;
                        }
                    }

                    // down-right
                    for (var j = index - 0x9; j % 0x8 != 0x7 && j >= 0x0; j -= 0x9)
                    {
                        validMoves |= Util.PowerLookup[j];
                        if ((bishopOccupancyVariations[index][variationIndex] & Util.PowerLookup[j]) != 0x0)
                        {
                            break;
                        }
                    }

                    // down-left
                    for (var j = index - 0x7; j % 0x8 != 0x0 && j >= 0x0; j -= 0x7)
                    {
                        validMoves |= Util.PowerLookup[j];
                        if ((bishopOccupancyVariations[index][variationIndex] & Util.PowerLookup[j]) != 0x0)
                        {
                            break;
                        }
                    }

                    BishopMagics[index].MagicMoves[magicIndex] = validMoves;
                }
            }
        }
    }
}