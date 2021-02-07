using System;
using System.Collections.Generic;

namespace UCIProtocol
{
    public static class UCIQuery
    {
        public static UCIQueryToken IsReady()
        {
            return new IsReadyCommand();
        }

        /// <summary>
        /// Returns a Go command with infinite search.
        /// </summary>
        /// <returns></returns>
        public static UCIQueryToken Go()
        {
            return new GoCommand();
        }

        /// <summary>
        /// Returns a Go command with the movetime option.
        /// </summary>
        /// <param name="time">How long, in milliseconds, the engine should search.</param>
        /// <returns></returns>
        public static UCIQueryToken Go(int time)
        {
            return new GoCommand(time);
        }

        public static UCIQueryToken UciNewGame()
        {
            return new UciNewGame();
        }

        public static UCIQueryToken PonderHit()
        {
            return new PonderHitCommand();
        }

        /// <summary>
        /// Returns a Position command with the fenstring option.
        /// </summary>
        /// <param name="fenString">A FEN formatted string of the position.</param>
        /// <param name="moves">A list of moves to play on the given position.</param>
        /// <returns></returns>
        public static UCIQueryToken Position(string fenString, List<string> moves)
        {
            return new PositionCommand(fenString, moves);
        }

        /// <summary>
        /// Returns a Position command starting with the chess initial setup.
        /// </summary>
        /// <param name="moves">A list of moves to play on the initial position.</param>
        /// <returns></returns>
        public static UCIQueryToken Position(List<string> moves)
        {
            return new PositionCommand(moves);
        }

        /// <summary>
        /// Returns an Option command.
        /// </summary>
        /// <param name="name">Name of the option.</param>
        /// <param name="value">Value of the option.</param>
        /// <returns></returns>
        public static UCIQueryToken SetOption(string name, string value)
        {
            return new SetOptionCommand(name, value);
        }

        public static UCIQueryToken Stop()
        {
            return new StopCommand();
        }

        public static UCIQueryToken Quit()
        {
            return new QuitCommand();
        }

        public static UCIQueryToken Uci()
        {
            return new Uci();
        }
    }

    internal class GoCommand : UCIQueryToken
    {
        public GoCommand() { }

        public GoCommand(int time)
        {
            this.Time = time;
        }

        private string _name = "go";
        public override string Name { get { return _name; } }

        private int Time { get; } = -1;

        public override string ToString()
        {
            return $"{_name} {(Time == -1 ? "infinite" : $"movetime {Time}")}";
        }
    }

    internal class IsReadyCommand : UCIQueryToken
    {
        private string _name = "isready";

        public override string Name { get { return _name; } }

        public override string ToString()
        {
            return _name;
        }
    }

    internal class PonderHitCommand : UCIQueryToken
    {
        private string _name = "ponderhit";

        public override string Name { get { return _name; } }

        public override string ToString()
        {
            return _name;
        }
    }

    internal class PositionCommand : UCIQueryToken
    {
        public PositionCommand(string fenString, List<string> moves)
        {
            this.FenString = fenString;
            this.Moves.AddRange(moves);
        }

        public PositionCommand(List<string> moves)
        {
            this.Moves.AddRange(moves);
        }

        private string FenString { get; } = string.Empty;

        private List<string> Moves { get; } = new List<string>();

        private string _name = "position";
        public override string Name { get { return _name; } }

        public override string ToString()
        {
            var moves = string.Empty;
            foreach (string move in Moves)
            {
                moves += $"{move} ";
            }
            if (moves.EndsWith(" ")) { moves = moves.TrimEnd(new char[] { ' ' }); }

            if (!string.IsNullOrWhiteSpace(FenString))
            {
                return $"{_name} fen {FenString}{(string.IsNullOrWhiteSpace(moves) ? string.Empty : $" moves {moves}")}";
            }
            else
            {
                return $"{_name} startpos{(string.IsNullOrWhiteSpace(moves) ? string.Empty : $" moves {moves}")}";
            }
        }
    }

    internal class Uci : UCIQueryToken
    {
        private string _name = "uci";

        public override string Name { get { return _name; } }

        public override string ToString()
        {
            return _name;
        }
    }

    internal class UciNewGame : UCIQueryToken
    {
        private string _name = "ucinewgame";

        public override string Name { get { return _name; } }

        public override string ToString()
        {
            return _name;
        }
    }

    internal class SetOptionCommand : UCIQueryToken
    {
        public SetOptionCommand(string name, string value)
        {
            this.OptionName = name;
            this.OptionValue = value;
        }

        private string _name = "setoption";
        public override string Name { get { return _name; } }

        private string OptionName { get; set; } = string.Empty;

        private string OptionValue { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{_name} {OptionName}{(string.IsNullOrWhiteSpace(OptionValue) ? string.Empty : OptionValue)}";
        }
    }

    internal class StopCommand : UCIQueryToken
    {
        private string _name = "stop";

        public override string Name { get { return _name; } }

        public override string ToString()
        {
            return _name;
        }
    }

    internal class QuitCommand : UCIQueryToken
    {
        private string _name = "quit";

        public override string Name { get { return _name; } }

        public override string ToString()
        {
            return _name;
        }
    }

}
