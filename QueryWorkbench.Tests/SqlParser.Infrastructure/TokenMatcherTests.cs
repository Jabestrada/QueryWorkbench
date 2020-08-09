using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlParser.Infrastructure.Tokenizing;
using System.Linq;

namespace QueryWorkbench.Tests.SqlParser.Infrastructure {
    [TestClass]
    public class TokenMatcherTests {

        [DataTestMethod]
        [DataRow("ab--c", 1, 2, "--")]
        [DataRow("--abc", 1, 0, "--")]
        [DataRow("abc--", 1, 3, "--")]
        [DataRow("ab**c", 1, 2, "**")]
        [DataRow("a---", 1, 1, "--")]
        [DataRow("abc-de", 0, -1, "--")]
        [DataRow("abc--de--", 2, 3, "--")]
        [DataRow("/*--abc*/---", 2, 2, "--")]

        public void itShouldMatchAndCount(string inputString, int expectedMatchCount,
                                          int expectedFirstIndex, string tokenToMatch) {
            var sut = new TokenMatcher(tokenToMatch);
            for (int k = 0; k < inputString.Length; k++) {
                sut.TryMatchChar(inputString[k], k);
            }

            Assert.AreEqual(expectedMatchCount, sut.GetMatchedIndices().Count());
            if (expectedMatchCount > 0) {
                Assert.AreEqual(expectedFirstIndex, sut.GetMatchedIndices().First());
            }
        }

        [DataTestMethod]
        [DataRow("--%", false, true, false)]
        public void itShouldReturnTrueIfMatchFound(string inputString,  
                                                   bool iteration1ExpectedResult,
                                                   bool iteration2ExpectedResult,
                                                   bool iteration3ExpectedResult) {
            
            Assert.IsTrue(inputString.Length == 3, "Unexpected input length");

            var matchResults = new bool[inputString.Length];
            var sut = new TokenMatcher("--");
            for (int k = 0; k < inputString.Length; k++) {
                matchResults[k] = sut.TryMatchChar(inputString[k], k);
            }

            Assert.AreEqual(iteration1ExpectedResult, matchResults[0]);
            Assert.AreEqual(iteration2ExpectedResult, matchResults[1]);
            Assert.AreEqual(iteration3ExpectedResult, matchResults[2]);
        }
    }
}
