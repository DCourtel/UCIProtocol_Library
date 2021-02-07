using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCIProtocol
{
    public static class UCIResponse
    {
        /// <summary>
        /// Returns an <see cref="UCIResponseToken"/> from an UCI response made by the chess engine.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static UCIResponseToken GetToken(string response)
        {
            response = RemoveExtraCharacters(response);

            if (response.ToLower().StartsWith("id "))
            {
                return new Id(response);
            }
            else if (response.ToLower().StartsWith("uciok"))
            {
                return new UciOk();
            }
            else if (response.ToLower().StartsWith("readyok"))
            {
                return new ReadyOk();
            }
            else if (response.ToLower().StartsWith("bestmove "))
            {
                return new BestMove(response);
            }

            return new UnknownUCIToken(response);
        }

        private static string RemoveExtraCharacters(string input)
        {
            //  Remove any tab characters
            input = input.Replace("\t", "");

            //  Remove any \r
            input = input.Replace("\r", "");

            // Remove any \n
            input = input.Replace("\n", "");

            //  Remove any extra spaces
            input = input.TrimStart();
            input = input.TrimEnd();
            while (input.Contains("  "))
            {
                input = input.Replace("  ", " ");
            }

            return input;
        }
    }

    public class Id : UCIResponseToken
    {
        public enum optionName
        {
            author,
            name
        }

        internal Id(string response)
        {
            //  id name Stockfish 12
            //  id author the Stockfish developers (see AUTHORS file)

            if (response.ToLower().StartsWith("id name "))
            {
                this.EngineName = response.Substring("id name ".Length);
                this.OptionType = optionName.name;
            }
            else if (response.ToLower().StartsWith("id author "))
            {
                this.EngineAuthor = response.Substring("id author ".Length);
                this.OptionType = optionName.author;
            }
            else { throw new ArgumentException("Unable to build an Id token with the given parameter."); }
        }

        public string EngineName { get; private set; }

        public string EngineAuthor { get; private set; }

        public optionName OptionType { get; private set; }

        public override string ToString()
        {
            return $"id {(OptionType == optionName.name ? $"name {EngineName}" : $"author {EngineAuthor}")}";
        }
    }

    public class UciOk : UCIResponseToken
    {
        public override string ToString()
        {
            return "uciok";
        }
    }

    public class ReadyOk : UCIResponseToken
    {
        public override string ToString()
        {
            return "readyok";
        }
    }

    public class BestMove : UCIResponseToken
    {
        internal BestMove(string response)
        {
            //  bestmove g1f3
            //  bestmove g1f3 ponder b8c6
            //  bestmove f7f8q                  : Promotion
            //  bestmove f7f8q ponder g2g1q     : Promotion
            var bestmoveWithoutPromotion = System.Text.RegularExpressions.Regex.Match(response, "^bestmove [a-h]{1}[1-8]{1}[a-h]{1}[1-8]{1}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var bestmoveWithPromotion = System.Text.RegularExpressions.Regex.Match(response, "^bestmove [a-h]{1}(7|2){1}[a-h]{1}(1|8){1}(r|b|n|q){1}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var ponderWithoutPromotion = System.Text.RegularExpressions.Regex.Match(response, "ponder [a-h]{1}[1-8]{1}[a-h]{1}[1-8]{1}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var ponderWithPromotion = System.Text.RegularExpressions.Regex.Match(response, "ponder [a-h]{1}(7|2){1}[a-h]{1}(1|8){1}(r|b|n|q){1}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (bestmoveWithPromotion.Success)
            {
                this.BestMoveValue = bestmoveWithPromotion.ToString().Substring("bestmove ".Length, 5);
            }
            else if (bestmoveWithoutPromotion.Success)
            {
                this.BestMoveValue = bestmoveWithoutPromotion.ToString().Substring("bestmove ".Length, 4);
            }
            else { throw new ArgumentException("Unable to find the bestmove token."); }

            if (ponderWithPromotion.Success)
            {
                this.PonderValue = ponderWithPromotion.ToString().Substring("ponder ".Length, 5);
            }
            else if (ponderWithoutPromotion.Success)
            {
                this.PonderValue = ponderWithoutPromotion.ToString().Substring("ponder ".Length, 4);
            }
        }

        public string BestMoveValue { get; private set; } = string.Empty;

        public string PonderValue { get; private set; } = string.Empty;

        public override string ToString()
        {
            return $"bestmove {BestMoveValue}{(!string.IsNullOrEmpty(PonderValue) ? $" ponder {PonderValue}" : string.Empty)}";
        }
    }

    public class UnknownUCIToken : UCIResponseToken
    {
        internal UnknownUCIToken(string input)
        {
            _input = input;
        }

        private string _input = string.Empty;
        public override string ToString()
        {
            return _input;
        }
    }
}
