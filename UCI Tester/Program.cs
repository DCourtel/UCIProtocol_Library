using UCIProtocol;
using System;
using System.Collections.Generic;

namespace UCI_Tester
{
    class Program
    {
        private static string enginePath = @"..\..\..\stockfish_20090216_x64_avx2.exe";

        static void Main(string[] args)
        {
            using (var stockfish = new UCIEngine(enginePath, false))
            {
                Console.WriteLine("Starting Stockfish engine");
                //  Get welcome response
                PrintResponse(stockfish.GetResponse());

                //  Send UCI command
                Console.WriteLine($"-> {stockfish.SendCommand(UCIQuery.Uci())}");
                PrintResponse(stockfish.GetResponse());

                //  Send IsReady command
                Console.WriteLine($"-> {stockfish.SendCommand(UCIQuery.IsReady())}");
                PrintResponse(stockfish.GetResponse());

                //  Send Position command
                Console.WriteLine($"-> {stockfish.SendCommand(UCIQuery.Position(new List<string>() { "e2e4", "e7e5" }))}");

                //  Send Go command
                Console.WriteLine($"-> {stockfish.SendCommand(UCIQuery.Go(5000))}");

                //  Display Stockfish’s thinks
                while (true)
                {
                    var tokens = stockfish.GetResponse();
                    if (tokens.Count > 0)
                    {
                        PrintResponse(tokens);
                        if (tokens[tokens.Count - 1] is BestMove) { break; }
                    }
                }
            }
            Console.WriteLine("End of the demo.");
            Console.Read();
        }

        private static void PrintResponse(List<UCIResponseToken> tokens)
        {
            foreach (UCIResponseToken token in tokens)
            {
                Console.WriteLine($"<- {token}");
            }
        }
    }
}
