using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using SUT = UCIProtocol.UCIResponse;

namespace UnitTestProject_UCIProtocol
{
    /// <summary>
    /// Description résumée pour UnitTest_UciResponse
    /// </summary>
    [TestClass]
    public class UnitTest_UCIResponse
    {
        [TestMethod]
        [DataRow("id author David", UCIProtocol.Id.optionName.author, "David")]
        [DataRow("  id    author   David  ", UCIProtocol.Id.optionName.author, "David")]
        [DataRow("\t\tid \tauthor\t David\t", UCIProtocol.Id.optionName.author, "David")]
        [DataRow("id name Stockfish 12", UCIProtocol.Id.optionName.name, "Stockfish 12")]
        [DataRow("  id    name   Stockfish 12  ", UCIProtocol.Id.optionName.name, "Stockfish 12")]
        [DataRow("\t\tid \tname\t Stockfish 12\t", UCIProtocol.Id.optionName.name, "Stockfish 12")]
        public void GetToken_Should_Retun_Id(string input, UCIProtocol.Id.optionName optionName, string expectedValue)
        {
            //	Arrange
            UCIProtocol.UCIResponseToken actual;

            //	Act
            actual = SUT.GetToken(input);

            //	Assert
            Assert.AreEqual(typeof(UCIProtocol.Id), actual.GetType());
            Assert.AreEqual(expectedValue, (optionName == UCIProtocol.Id.optionName.author ? (actual as UCIProtocol.Id).EngineAuthor : (actual as UCIProtocol.Id).EngineName));
        }

        [TestMethod]
        [DataRow("uciok")]
        [DataRow("   uciok   ")]
        [DataRow("\tuciok\t")]
        [DataRow("  \t  uciok  \t")]
        public void GetToken_Should_Retun_UciOk(string input)
        {
            //	Arrange
            UCIProtocol.UCIResponseToken actual;

            //	Act
            actual = SUT.GetToken(input);

            //	Assert
            Assert.AreEqual(typeof(UCIProtocol.UciOk), actual.GetType());
        }

        [TestMethod]
        [DataRow("readyok")]
        [DataRow("   readyok   ")]
        [DataRow("\treadyok\t")]
        [DataRow("  \t  readyok  \t")]
        public void GetToken_Should_Retun_ReadyOk(string input)
        {
            //	Arrange
            UCIProtocol.UCIResponseToken actual;

            //	Act
            actual = SUT.GetToken(input);

            //	Assert
            Assert.AreEqual(typeof(UCIProtocol.ReadyOk), actual.GetType());
        }

        [TestMethod]
        [DataRow("bestmove g1f3", "g1f3","")]
        [DataRow("bestmove f7f8q","f7f8q","")]
        [DataRow("bestmove g1f3 ponder b8c6", "g1f3", "b8c6")]
        [DataRow("bestmove g1f3 ponder g2g1q", "g1f3", "g2g1q")]
        [DataRow("bestmove f7f8q ponder g2g1q", "f7f8q", "g2g1q")]
        public void GetToken_Should_Retun_BestMove(string input, string expectedBestMove, string expectedPonder)
        {
            //	Arrange
            UCIProtocol.UCIResponseToken actual;

            //	Act
            actual = SUT.GetToken(input);

            //	Assert
            Assert.AreEqual(typeof(UCIProtocol.BestMove), actual.GetType());
            Assert.AreEqual(expectedBestMove, (actual as UCIProtocol.BestMove).BestMoveValue);
            Assert.AreEqual(expectedPonder, (actual as UCIProtocol.BestMove).PonderValue);
        }
    }
}
