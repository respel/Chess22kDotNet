using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chess22kDotNet.Engine;

namespace Chess22kDotNet.Search
{
    public static class SearchUtil
    {
        public static void Start(ChessBoard cb)
        {
            NegamaxUtil.IsRunning = true;
            cb.MoveCount = 0;

            if (UciOptions.ThreadCount == 1)
            {
                new SearchThread(0).Call();
            }
            else
            {
                var tasks = new List<Task>();
                for (var i = 0; i < UciOptions.ThreadCount; i++)
                {
                    if (i > 0)
                    {
                        ChessBoardUtil.Copy(cb, ChessBoardInstances.Get(i));
                    }

                    var i1 = i;
                    var t = new Task(() => new SearchThread(i1).Call());
                    tasks.Add(t);
                }

                try
                {
                    tasks.ForEach(task => task.Start());
                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}