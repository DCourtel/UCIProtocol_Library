using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using UCIProtocol;
using SUT = UCIProtocol.UCIQuery;

namespace UnitTestProject_UCIProtocol
{
    /// <summary>
    /// Description résumée pour UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest_UCIQuery
    {
        private UCIEngine _engine;
        private const string _stockfishPath = @"..\..\..\stockfish_20090216_x64_avx2.exe";

        private TestContext testContextInstance;

        /// <summary>
        ///Obtient ou définit le contexte de test qui fournit
        ///des informations sur la série de tests active, ainsi que ses fonctionnalités.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Attributs de tests supplémentaires

        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }

        // [ClassCleanup()]
        // public static void MyClassCleanup() { }

        [TestInitialize()]
        public void MyTestInitialize()
        {
            var engineFile = new System.IO.FileInfo(_stockfishPath);
            if (!engineFile.Exists)
            { throw new System.IO.FileNotFoundException(); }
            _engine = new UCIEngine(_stockfishPath, true);
        }
                
        [TestCleanup()]
        public void MyTestCleanup()
        {
            _engine.Dispose();
        }

        #endregion

        [TestMethod]
        public void UciCommands_Should_Return_ExpectedOutput()
        {
            //	Arrange
            List<UCIResponseToken> response;

            //	Act
            _engine.SendCommand(UCIQuery.Uci());
            response = _engine.GetResponse();

            Assert.IsTrue(response[0] is Id);
            Assert.IsTrue(response[1] is Id);
            Assert.AreEqual("Stockfish 12", (response[0] as Id).EngineName);
            Assert.AreEqual("the Stockfish developers (see AUTHORS file)", (response[1] as Id).EngineAuthor);

             _engine.SendCommand(UCIQuery.IsReady());
            response = _engine.GetResponse();
            Assert.IsTrue(response[0] is ReadyOk);

            _engine.SendCommand(UCIQuery.UciNewGame());
            response = _engine.GetResponse();
            Assert.AreEqual(0, response.Count);

            _engine.SendCommand(UCIQuery.Position(new List<string>() { "e2e4", "e7e5"}));
            response = _engine.GetResponse();
            Assert.AreEqual(0, response.Count);

            _engine.SendCommand(UCIQuery.Go());
            System.Threading.Thread.Sleep(2500);
            _engine.SendCommand(UCIQuery.Stop());
            System.Threading.Thread.Sleep(250); //  Gives the engine enough time to finish.
            response = _engine.GetResponse();
            Assert.IsTrue(response[response.Count - 1] is BestMove);
            Assert.IsFalse(string.IsNullOrEmpty((response[response.Count - 1] as BestMove).BestMoveValue));
            Assert.IsFalse(string.IsNullOrEmpty((response[response.Count - 1] as BestMove).PonderValue));

            _engine.SendCommand(UCIQuery.Quit());
        }
    }
}
