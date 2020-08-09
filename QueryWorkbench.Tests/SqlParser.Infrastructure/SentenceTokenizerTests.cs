using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlParser.Infrastructure.CodeComments;
using SqlParser.Infrastructure.Tokenizing;
using System.Linq;

namespace QueryWorkbench.Tests.SqlTokenizer.Infrastructure {

    [TestClass]
    public class SentenceTokenizerTests {

        [DataTestMethod]
        [DataRow("abc", 0, "--")]
        [DataRow("ab--c", 1, "--")]
        [DataRow("--abc", 1, "--")]
        [DataRow("abc--", 1, "--")]
        [DataRow("a--b--c--", 3, "--")]
        [DataRow("ab--c", 0, "---")]   // Custom non-block comment token negative test case
        [DataRow("ab**c", 1, "**")]    // Custom non-block comment token positive test case
        [DataRow("a---", 1, "--")]
        [DataRow("a----", 2, "--")]
        public void itShouldTokenizeNonCommentBlockIndices(string word,
                                                        int expectedNonBlockCommentIndexCount,
                                                        string nonBlockCommentToken) {
            var sut = new SentenceTokenizer(nonBlockCommentToken);
            var result = sut.Tokenize(word);
            Assert.IsNotNull(result.FirstOrDefault());
            Assert.AreEqual(expectedNonBlockCommentIndexCount, result.FirstOrDefault().CodeCommentIndices.Count());
        }

        [DataTestMethod]
        [DataRow("/*abc*/", 1, 0, 6, -1, -1)]
        [DataRow("abc*//*", 2, null, 4, 5, null)]
        [DataRow("/*abc*//*", 2, 0, 6, 7, null)]
        [DataRow("abc", 0, -1, -1, -1, -1)]
        [DataRow("/abc", 0, -1, -1, -1, -1)]
        public void itShouldTokenizeCommentBlockIndices(string word,
                                                     int expectedCommentBlockIndicesCount,
                                                      int? expectedFirstCommentBlockIndexStart,
                                                      int? expectedFirstCommentBlockIndexEnd,
                                                      int? expectedSecondCommentBlockIndexStart,
                                                      int? expectedSecondCommentBlockIndexEnd) {
            var sut = new SentenceTokenizer();
            var result = sut.Tokenize(word);
            Assert.IsNotNull(result.FirstOrDefault());
            Assert.AreEqual(expectedCommentBlockIndicesCount,
                            result.FirstOrDefault().CodeCommentIndices.Count());

            var blockCommentIndices = result.FirstOrDefault().CodeCommentIndices.ToList();
            if (expectedCommentBlockIndicesCount > 0) {
                Assert.AreEqual(expectedFirstCommentBlockIndexStart, blockCommentIndices[0].StartIndex);
                Assert.AreEqual(expectedFirstCommentBlockIndexEnd, blockCommentIndices[0].EndIndex);
            }

            if (expectedCommentBlockIndicesCount > 1) {
                Assert.AreEqual(expectedSecondCommentBlockIndexStart, blockCommentIndices[1].StartIndex);
                Assert.AreEqual(expectedSecondCommentBlockIndexEnd, blockCommentIndices[1].EndIndex);
            }
        }


        [DataTestMethod]
        [DataRow(new string[] { "the quick brown fox",
                                "the", "quick", "brown", "fox" })]
        [DataRow(new string[] { " the    quick  brown  fox ",
                                "the", "quick", "brown", "fox" })]
        [DataRow(new string[] { "SELECT * FROM Person P",
                                "SELECT", "*", "FROM", "Person", "P" })]
        [DataRow(new string[] { "SELECT * FROM Person -- P",
                                "SELECT", "*", "FROM", "Person", "--", "P" })]
        public void itShouldTokenizeWordBoundariesDelimitedByOneOrMoreSpaces(string[] inputs) {
            var sentence = inputs[0];
            var sut = new SentenceTokenizer();
            var result = sut.Tokenize(sentence).ToArray();
            Assert.AreEqual(inputs.Length - 1, result.Length);
            for (int j = 0; j < result.Length; j++) {
                Assert.AreEqual(inputs[j + 1], result[j].Text);
            }
        }


        [TestMethod]
        public void itShouldTokenizeWordBoundariesWithBlockAndNonBlockComments_TestCase_1() {
            var input = "SELECT * FROM /* ---Person */ a/b*c";
            var sut = new SentenceTokenizer();

            var results = sut.Tokenize(input).ToList();

            Assert.AreEqual(7, results.Count());

            Token TokenizedWord = results[3];
            // results[3] = /*
            var blockCommentIndices = TokenizedWord.CodeCommentIndices
                                                .Where(ci => ci.CommentType == CodeCommentType.Block).ToList();
            var nonBlockCommentIndices = TokenizedWord.CodeCommentIndices
                                                   .Where(ci => ci.CommentType == CodeCommentType.Inline).ToList();
            Assert.AreEqual(0, nonBlockCommentIndices.Count());
            Assert.AreEqual(1, blockCommentIndices.Count());
            Assert.AreEqual(14, blockCommentIndices[0].StartIndex);
            Assert.AreEqual(null, blockCommentIndices[0].EndIndex);

            // results[4] = --Person
            TokenizedWord = results[4];
            Assert.AreEqual(0, TokenizedWord.CodeCommentIndices.Where(ci => ci.CommentType == CodeCommentType.Block).Count());
            nonBlockCommentIndices = TokenizedWord.CodeCommentIndices
                                               .Where(ci => ci.CommentType == CodeCommentType.Inline).ToList();
            Assert.AreEqual(1, nonBlockCommentIndices.Count());
            Assert.AreEqual(17, nonBlockCommentIndices[0].StartIndex);

            // results[5] = */
            TokenizedWord = results[5];
            nonBlockCommentIndices = TokenizedWord.CodeCommentIndices
                                               .Where(ci => ci.CommentType == CodeCommentType.Inline).ToList();
            Assert.AreEqual(0, nonBlockCommentIndices.Count());
            blockCommentIndices = TokenizedWord.CodeCommentIndices
                                            .Where(ci => ci.CommentType == CodeCommentType.Block).ToList();
            Assert.AreEqual(1, blockCommentIndices.Count());
            Assert.AreEqual(null, blockCommentIndices[0].StartIndex);
            Assert.AreEqual(28, blockCommentIndices[0].EndIndex);

            // results[6] =  a/b*c
            TokenizedWord = results[6];
            nonBlockCommentIndices = TokenizedWord.CodeCommentIndices
                                            .Where(ci => ci.CommentType == CodeCommentType.Inline).ToList();
            Assert.AreEqual(0, nonBlockCommentIndices.Count());
            blockCommentIndices = TokenizedWord.CodeCommentIndices
                                .Where(ci => ci.CommentType == CodeCommentType.Block).ToList();
            Assert.AreEqual(0, blockCommentIndices.Count());
        }


        //https://stackoverflow.com/questions/57609039/how-to-supply-two-arrays-as-datarow-parameters

        //[DynamicData(nameof(TestDataGenerator), DynamicDataSourceType.Method)]
        // [TestMethod] public void MyTestMethod(etc...) { ... }

        //static IEnumerable<object[]> TestDataGenerator() {
        //    return new[] {
        //        new[] { "the quick brown fox", new[] { "the", "quick", "brown", "fox" } }
        //    };
        //}
    }
}
