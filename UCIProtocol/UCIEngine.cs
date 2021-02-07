using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace UCIProtocol
{
    public class UCIEngine : IDisposable
    {
        private Process _engineProcess;
        private System.Threading.Thread _uciQueryThread;
        private object _bufferLocker = new object();

        /// <summary>
        /// A class to read from/write to an UCI compatible chess engine.
        /// </summary>
        /// <param name="enginePath">Full path to the EXE file of the chess engine.</param>
        /// <param name="discardWelcomeMessage">Whether or not the first message should be discarded.</param>
        public UCIEngine(string enginePath, bool discardWelcomeMessage)
        {
            var engineFile = new System.IO.FileInfo(enginePath);
            if (!engineFile.Exists) { throw new System.IO.FileNotFoundException(); }
            ProcessStartInfo startInfo = new ProcessStartInfo(enginePath)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };
            _engineProcess = new Process() { StartInfo = startInfo };
            _engineProcess.Start();

            System.Threading.Thread.Sleep(250);
            if (discardWelcomeMessage)
            {
                _engineProcess.StandardOutput.ReadLine();
            }
            var threadStart = new System.Threading.ThreadStart(ReadResponse);
            _uciQueryThread = new System.Threading.Thread(threadStart);
            _uciQueryThread.IsBackground = false;
            _uciQueryThread.Start();
        }

        #region Methods

        /// <summary>
        /// Returns how many response messages there currently is in the queue, if any.
        /// </summary>
        /// <returns></returns>
        public int GetAwaitingResponseCount()
        {
            lock (_bufferLocker)
            {
                return _responses.Count;
            }
        }

        private List<string> _responses = new List<string>();
        /// <summary>
        /// Returns a list of <see cref="UCIResponseToken"/> which each represent a response from the chess engine.
        /// </summary>
        /// <param name="firstMessageTimeout">How long the method should wait the first message, before considering there is no message in the queue. Default is 3000ms.</param>
        /// <returns></returns>
        public List<UCIResponseToken> GetResponse(int firstMessageTimeout = 3000)
        {
            while (_responses.Count == 0 && firstMessageTimeout > 0)
            {
                System.Threading.Thread.Sleep(100);
                firstMessageTimeout -= 100;
            }
            lock (_bufferLocker)
            {
                var tokens = new List<UCIResponseToken>();
                if (_responses.Count > 0)
                {
                    foreach (string response in _responses)
                    {
                        tokens.Add(UCIResponse.GetToken(response));
                    }
                    _responses.Clear();
                }

                return tokens;
            }
        }

        private void SetResponse(string value)
        {
            lock (_bufferLocker)
            {
                if (!string.IsNullOrEmpty(value)) { if (!string.IsNullOrEmpty(value)) { _responses.Add(value); } }
            }
        }

        private void ReadResponse()
        {
            do
            {
                SetResponse(_engineProcess.StandardOutput.ReadLine());
            } while (!_engineProcess.StandardOutput.EndOfStream);
        }

        /// <summary>
        /// Sends an <see cref="UCIQueryToken"/> to the chess engine.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string SendCommand(UCIQueryToken command)
        {
            var commandSent = command.ToString();
            _engineProcess.StandardInput.WriteLine(commandSent);

            return commandSent;
        }

        public void Dispose()
        {
            try
            {
                SendCommand(new StopCommand());
                System.Threading.Thread.Sleep(250);
                SendCommand(new QuitCommand());
                System.Threading.Thread.Sleep(250);
                _engineProcess.Close();
                _engineProcess.Dispose();
                _uciQueryThread.Abort();
            }
            catch (Exception) { }
        }

        #endregion Methods
    }
}
