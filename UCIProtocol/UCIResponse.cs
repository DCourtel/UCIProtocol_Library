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

            if (response.ToLower().StartsWith("id "))
            {
                return new Id(response);
            }
            else if (response.ToLower().StartsWith("uciok"))
            {
                return new UCIOk();
            }
            else if (response.ToLower().StartsWith("readyok"))
            {
                return new ReadyOk();
            }
            else if (response.ToLower().StartsWith("bestmove "))
            {
                return new BestMove(response);
            }
            else if (response.ToLower().StartsWith("option name "))
            {
                return new Option(response);
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

    public class Option : UCIResponseToken
    {
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
                var tokens = GetTokens(response);
                if (tokens.Count == 3)
                {
                    this.StringDefaultValue = tokens[2];
                }
            }
            else if (response.StartsWith("spin default ", StringComparison.InvariantCultureIgnoreCase))
            {
                //  option name Contempt type spin default 24 min -100 max 100
                this.OptionType = optionType.spin;
                var tokens = GetTokens(response);
                this.SpinDefaultValue = int.Parse(tokens[2]);
                this.SpinMinValue = int.Parse(tokens[4]);
                this.SpinMaxValue = int.Parse(tokens[6]);
            }
            else if (response.StartsWith("combo default ", StringComparison.InvariantCultureIgnoreCase))
            {
                //  option name Analysis Contempt type combo default Both var Off var White var Black var Both
                this.OptionType = optionType.combo;
                var tokens = GetTokens(response);
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
                var tokens = GetTokens(response);
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

        private List<string> GetTokens(string input)
        {
            return input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public override string ToString()
        {
            var optionContent = string.Empty;
            switch (OptionType)
            {
                case optionType.check:
                    optionContent = $" {CheckDefaultValue}";
                    break;
                case optionType.spin:
                    optionContent = $" {SpinDefaultValue} min {SpinMinValue} max {SpinMaxValue}";
                    break;
                case optionType.combo:
                    var comboValues = string.Empty;
                    foreach (var value in this.ComboValues)
                    {
                        comboValues += $" var {value}";
                    }
                    optionContent = $" {ComboDefaultValue}{comboValues}";
                    break;
                case optionType.button:
                    return $"option name {Name} type button";
                case optionType.@string:
                    optionContent = $"{(!string.IsNullOrEmpty(StringDefaultValue) ? $" {StringDefaultValue}" : string.Empty)}";
                    break;
            }
            return $"option name {Name} type {OptionType} default{optionContent}";
        }



        #endregion  Methods
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
