using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            if (response.StartsWith("id ", StringComparison.InvariantCultureIgnoreCase))
            {
                return new Id(response);
            }
            else if (response.StartsWith("uciok", StringComparison.InvariantCultureIgnoreCase))
            {
                return new UCIOk();
            }
            else if (response.StartsWith("readyok", StringComparison.InvariantCultureIgnoreCase))
            {
                return new ReadyOk();
            }
            else if (response.StartsWith("bestmove ", StringComparison.InvariantCultureIgnoreCase))
            {
                return new BestMove(response);
            }
            else if (response.StartsWith("option name ", StringComparison.InvariantCultureIgnoreCase))
            {
                return new Option(response);
            }
            else if (response.StartsWith("info ", StringComparison.InvariantCultureIgnoreCase))
            {
                return new Info(response);
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

        internal static List<string> GetTokens(string input)
        {
            return input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }

    public class Id : UCIResponseToken
    {
        private string _originalResponse;

        public enum optionName
        {
            author,
            name
        }

        internal Id(string response)
        {
            //  id name Stockfish 12
            //  id author the Stockfish developers (see AUTHORS file)
            this._originalResponse = response;

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
            return _originalResponse;
        }
    }

    public class UCIOk : UCIResponseToken
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
        private string _originalResponse;

        internal BestMove(string response)
        {
            //  bestmove g1f3
            //  bestmove g1f3 ponder b8c6
            //  bestmove f7f8q                  : Promotion
            //  bestmove f7f8q ponder g2g1q     : Promotion
            this._originalResponse = response;
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
            return _originalResponse;
        }
    }

    public class Option : UCIResponseToken
    {
        private string _originalResponse;

        public enum optionType
        {
            check,
            spin,
            combo,
            button,
            @string
        }

        internal Option(string response)
        {
            this._originalResponse = response;
            response = response.Substring("option name ".Length);
            var typeIndex = response.IndexOf(" type ", StringComparison.InvariantCultureIgnoreCase);
            if (typeIndex == -1) { throw new ArgumentException("Unable to find the «type» keyword in this option."); }
            this.Name = response.Substring(0, typeIndex);
            response = response.Substring(typeIndex + " type ".Length);
            if (response.StartsWith("string default", StringComparison.InvariantCultureIgnoreCase))
            {
                //  option name Debug Log File type string default
                //  option name EvalFile type string default nn-82215d0fd0df.nnue
                this.OptionType = optionType.@string;
                var tokens = UCIResponse.GetTokens(response);
                if (tokens.Count == 3)
                {
                    this.StringDefaultValue = tokens[2];
                }
            }
            else if (response.StartsWith("spin default ", StringComparison.InvariantCultureIgnoreCase))
            {
                //  option name Contempt type spin default 24 min -100 max 100
                this.OptionType = optionType.spin;
                var tokens = UCIResponse.GetTokens(response);
                this.SpinDefaultValue = int.Parse(tokens[2]);
                this.SpinMinValue = int.Parse(tokens[4]);
                this.SpinMaxValue = int.Parse(tokens[6]);
            }
            else if (response.StartsWith("combo default ", StringComparison.InvariantCultureIgnoreCase))
            {
                //  option name Analysis Contempt type combo default Both var Off var White var Black var Both
                this.OptionType = optionType.combo;
                var tokens = UCIResponse.GetTokens(response);
                this.ComboDefaultValue = tokens[2];
                var comboValues = new List<string>();
                for (int i = 4; i < tokens.Count; i += 2)
                {
                    comboValues.Add(tokens[i]);
                }
                this.ComboValues = new ReadOnlyCollection<string>(comboValues);
            }
            else if (response.StartsWith("button", StringComparison.InvariantCultureIgnoreCase))
            {
                //  option name Clear Hash type button
                this.OptionType = optionType.button;
            }
            else if (response.StartsWith("check default ", StringComparison.InvariantCultureIgnoreCase))
            {
                //  option name Ponder type check default false
                this.OptionType = optionType.check;
                var tokens = UCIResponse.GetTokens(response);
                this.CheckDefaultValue = bool.Parse(tokens[2]);
            }
            else
            {
                throw new ArgumentException("Unable to identify the type of this option.");
            }
        }

        public string Name { get; private set; } = string.Empty;

        public optionType OptionType { get; private set; }

        public bool CheckDefaultValue { get; private set; }

        public int SpinDefaultValue { get; private set; }

        public int SpinMinValue { get; private set; }

        public int SpinMaxValue { get; private set; }

        public string ComboDefaultValue { get; private set; }

        public ReadOnlyCollection<string> ComboValues { get; } = new ReadOnlyCollection<string>(new List<string>());

        public string StringDefaultValue { get; private set; } = string.Empty;

        #region Methods

        public override string ToString()
        {
            return _originalResponse;
        }

        #endregion  Methods
    }

    public class Info : UCIResponseToken
    {
        //  info depth 5 seldepth 5 multipv 1 score cp -15 nodes 825 nps 24264 tbhits 0 time 34 pv c7c5 g1f3 b8c6 d2d4 c5d4
        //  info depth 20 currmove g8f6 currmovenumber 3

        private string _originalResponse;

        internal Info(string response)
        {
            this._originalResponse = response;
            var tokens = UCIResponse.GetTokens(response.ToLower());
            for (int i = 0; i < tokens.Count; i++)
            {
                switch (tokens[i])
                {
                    case "depth":
                        i++;
                        Depth = int.Parse(tokens[i]);
                        break;
                    case "seldepth":
                        i++;
                        SelDepth = int.Parse(tokens[i]);
                        break;
                    case "multipv":
                        i++;
                        MultiPv = int.Parse(tokens[i]);
                        break;
                    case "score":
                        i++;
                        switch (tokens[i])
                        {
                            case "cp":
                                i++;
                                CpScore = int.Parse(tokens[i]);
                                if (tokens[i + 1] == "lowerbound") { IsLowerBounndScore = true; }
                                if (tokens[i + 1] == "upperbound") { IsUpperBoundScore = true; }
                                break;
                            case "mate":
                                i++;
                                MateScore = int.Parse(tokens[i]);
                                break;
                        }
                        break;
                    case "nodes":
                        i++;
                        Nodes = int.Parse(tokens[i]);
                        break;
                    case "nps":
                        i++;
                        Nps = int.Parse(tokens[i]);
                        break;
                    case "tbhits":
                        i++;
                        TbHits = int.Parse(tokens[i]);
                        break;
                    case "time":
                        i++;
                        Time = int.Parse(tokens[i]);
                        break;
                    case "pv":
                        i++;
                        var moves = new List<string>();
                        for (int j = i; j < tokens.Count; j++)
                        {
                            moves.Add(tokens[j]);
                        }
                        Pv = new ReadOnlyCollection<string>(moves);
                        break;
                    case "currmove":
                        i++;
                        CurrMove = tokens[i];
                        break;
                    case "currmovenumber":
                        i++;
                        CurrMoveNumber = int.Parse(tokens[i]);
                        break;
                }
            }
        }

        #region Properties

        public int Depth { get; private set; }

        public int SelDepth { get; private set; }

        public int Time { get; private set; }

        public int Nodes { get; private set; }

        public ReadOnlyCollection<string> Pv { get; } = new ReadOnlyCollection<string>(new List<string>());

        public int MultiPv { get; private set; }

        public int CpScore { get; private set; }

        public int MateScore { get; private set; }

        public bool IsLowerBounndScore { get; private set; }

        public bool IsUpperBoundScore { get; private set; }

        public string CurrMove { get; private set; } = string.Empty;

        public int CurrMoveNumber { get; private set; }

        public int HashFull { get; private set; }

        public int Nps { get; private set; }

        public int TbHits { get; private set; }

        public int CpuLoad { get; private set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return _originalResponse;
        }

        #endregion Methods
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
