using System;
using System.IO;
using Serilog;

namespace Chess22kDotNet.Engine
{
    public static class ErrorLogger
    {
        public static string StartFen = "";

        static ErrorLogger()
        {
            // setup logger
            const string dateFormat = "yyyy-MM-dd_HH.mm.ss.fff";
            var completeFilePath = Path.GetTempPath() + "Chess22kDotNet_" + DateTime.Now.ToString(dateFormat) + ".log";
            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.File(completeFilePath)
                .CreateLogger();
        }

        public static void Log(ChessBoard cb, Exception e, bool systemExit)
        {
            try
            {
                // print to Console
                Console.WriteLine(e);

                // redirect Console
                var writer = new StringWriter();
                Console.SetOut(writer);

                // print info
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Chess22kDotNet " + UciOut.GetVersion());
                Console.WriteLine();
                Console.WriteLine("start fen");
                Console.WriteLine(StartFen);
                Console.WriteLine();
                Console.WriteLine("crashed fen");
                Console.WriteLine(cb);
                Console.WriteLine();

                // print statistics
                Statistics.Print();

                Console.Out.Flush();

                // print exception
                Serilog.Log.Information(writer.ToString());
                Serilog.Log.Error(e, "An exception occurred");
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
                if (systemExit)
                {
                    Environment.Exit(1);
                }
            }
        }
    }
}