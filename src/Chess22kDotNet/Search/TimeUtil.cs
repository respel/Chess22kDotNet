using System;
using System.Diagnostics;
using Chess22kDotNet.Engine;

namespace Chess22kDotNet.Search
{
    public static class TimeUtil
    {
        private static readonly Stopwatch Stopwatch = new Stopwatch();

        private static int _movesToGo;
        private static int _moveCount;
        private static int _increment;
        private static long _timeWindowNs;
        private static long _totalTimeLeftMs;
        private static long _maxTimeMs;
        private static bool _isTtHit;
        private static bool _isExactMoveTime;

        static TimeUtil()
        {
            Reset();
        }

        public static void Reset()
        {
            Stopwatch.Restart();
            _isExactMoveTime = false;
            _movesToGo = -1;
            _totalTimeLeftMs = long.MaxValue;
            _maxTimeMs = long.MaxValue;
            _timeWindowNs = long.MaxValue;
            _increment = 0;
            _isTtHit = false;
        }

        public static void Start()
        {
            if (_isExactMoveTime)
                // we depend on the max-time thread
                return;

            if (_totalTimeLeftMs == long.MaxValue)
            {
                _timeWindowNs = long.MaxValue;
                return;
            }

            if (_movesToGo == -1)
            {
                var incrementWindow = _increment < _totalTimeLeftMs / 2 ? _increment / 2 : 0;
                if (_moveCount <= 40)
                    // first 40 moves get 50% of the total time
                    _timeWindowNs = 1_000_000 * (_totalTimeLeftMs / (80 - _moveCount) + incrementWindow);
                else
                    // every next move gets less and less time
                    _timeWindowNs = 1_000_000 * (_totalTimeLeftMs / 50 + incrementWindow / 2);
            }
            else
            {
                // if we have more than 50% of the time left, continue with next ply
                _timeWindowNs = 1_000_000 * _totalTimeLeftMs / _movesToGo / 2;
            }

            if (!_isTtHit) _timeWindowNs *= 2;

            _maxTimeMs = _movesToGo switch
            {
                1 => Math.Max(50, _totalTimeLeftMs - 200),
                2 or 3 or 4 => _totalTimeLeftMs / _movesToGo,
                _ => _timeWindowNs / 1_000_000 * 4
            };
        }

        public static long GetMaxTimeMs()
        {
            return _maxTimeMs;
        }

        public static void SetExactMoveTime(int moveTimeMs)
        {
            _isExactMoveTime = true;
            _maxTimeMs = moveTimeMs;
        }

        public static void SetSimpleTimeWindow(long thinkingTimeMs)
        {
            // if we have more than 50% of the time left, continue with next ply
            _timeWindowNs = 1_000_000 * thinkingTimeMs / 2;
        }

        public static bool IsTimeLeft()
        {
            if (_isExactMoveTime) return true;

            if (MainEngine.Pondering) return true;

            return Stopwatch.Elapsed.TotalMilliseconds * 1000000 < _timeWindowNs;
        }

        public static long GetPassedTimeMs()
        {
            return Stopwatch.ElapsedMilliseconds;
        }

        public static void SetMovesToGo(int movesToGo)
        {
            _movesToGo = movesToGo;
        }

        public static void SetTotalTimeLeft(int totalTimeLeftMs)
        {
            _totalTimeLeftMs = totalTimeLeftMs;
        }

        public static void SetMoveCount(int moveCount)
        {
            _moveCount = moveCount;
        }

        public static void SetTtHit()
        {
            _isTtHit = true;
        }

        public static void SetIncrement(int increment)
        {
            _increment = increment;
        }
    }
}