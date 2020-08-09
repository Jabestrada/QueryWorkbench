using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlParser.Infrastructure.Tokenizing2;
using System;
using System.Linq;

namespace QueryWorkbench.Tests.Tokenizing2Tests {
    [TestClass]
    public class Tokernizer2Tests {
        [DataTestMethod]
        [DataRow("the", 1, 0)]
        [DataRow(" ", 1, 1)]
        [DataRow("     ", 1, 1)]
        [DataRow("the ", 2, 1)]
        [DataRow(" the ", 3, 2)]
        [DataRow("the quick brown fox jumped ", 10, 5)]
        [DataRow(" the quick brown fox jumped ", 11, 6)]
        public void itShouldCountTokens(string input,
                                                int expectedTokenCount,
                                                int expectedWhitespaceTokenCount) {
            var sut = new Tokenizer2();

            var result = sut.Tokenize(input);

            Assert.AreEqual(expectedTokenCount, result.Count());
            Assert.AreEqual(expectedWhitespaceTokenCount,
                            result.Where(r => r.TokenType == TokenTypeEnum.Whitespace).Count());
        }

        [DataTestMethod]
        [DataRow("the", 0, 2)]
        [DataRow(" ", 0, 0)]
        [DataRow("     ", 0, 4)]
        [DataRow("123456", 0, 5)]
        public void itShouldSetTextAndIndices(string input,
                                                int expectedStartIndex,
                                                int expectedEndIndex) {
            var sut = new Tokenizer2();

            var result = sut.Tokenize(input);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(input, result.First().Text);
            Assert.AreEqual(expectedStartIndex, result.First().StartIndex);
            Assert.AreEqual(expectedEndIndex, result.First().EndIndex);
        }

        [DataTestMethod]
        [DataRow("--", 0, 1)]
        [DataRow("--a", 0, 2)]
        [DataRow("--a b", 0, 4)]
        [DataRow("--*/", 0, 3)]
        [DataRow("---", 0, 2)]
        [DataRow("-- ", 0, 2)]
        public void itShouldTokenizeCommentsAtStartOfInput(string input,
                                                int expectedStartIndex,
                                                int expectedEndIndex) {
            var sut = new Tokenizer2();

            var result = sut.Tokenize(input);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(input, result.First().Text);
            Assert.AreEqual(expectedStartIndex, result.First().StartIndex);
            Assert.AreEqual(expectedEndIndex, result.First().EndIndex);
        }

        [DataTestMethod]
        [DataRow("the--", "the", "--", TokenTypeEnum.UnclassifiedIdentifier, TokenTypeEnum.InlineComment)]
        [DataRow("the--abc", "the", "--abc", TokenTypeEnum.UnclassifiedIdentifier, TokenTypeEnum.InlineComment)]
        [DataRow("the--abc ", "the", "--abc ", TokenTypeEnum.UnclassifiedIdentifier, TokenTypeEnum.InlineComment)]
        [DataRow("the---abc ", "the", "---abc ", TokenTypeEnum.UnclassifiedIdentifier, TokenTypeEnum.InlineComment)]
        public void itShouldTokenizeInlineCommentsNotAtStartOfInput(string input,
                                                string expectedToken1Text,
                                                string expectedToken2Text,
                                                TokenTypeEnum expectedToken1Type,
                                                TokenTypeEnum expectedToken2Type) {
            var sut = new Tokenizer2();

            var result = sut.Tokenize(input);

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(expectedToken1Text, result.First().Text);
            Assert.AreEqual(expectedToken2Text, result.Last().Text);
            Assert.AreEqual(expectedToken1Type, result.First().TokenType);
            Assert.AreEqual(expectedToken2Type, result.Last().TokenType);
        }

        [TestMethod]
        public void itShouldTerminateInlineCommentsOnNewLine() {
            var input = "--abc " + Environment.NewLine + "a";

            var sut = new Tokenizer2();

            var result = sut.Tokenize(input);

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(TokenTypeEnum.InlineComment, result.First().TokenType);

        }

        [DataTestMethod]
        [DataRow("--/*--*/--", "--", "/*--*/", "--")]
        [DataRow("--/*--*/ab", "--", "/*--*/", "ab")]
        [DataRow("--/*--*/  ", "--", "/*--*/", "  ")]
        public void itShouldTerminateInlineCommentsOnBlockCommentStart(
                                string input,
                                string expectedText1,
                                string expectedText2,
                                string expectedText3) {
            var sut = new Tokenizer2();

            var result = sut.Tokenize(input);
            var results = result.ToList();

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(expectedText1, results[0].Text);
            Assert.AreEqual(expectedText2, results[1].Text);
            Assert.AreEqual(expectedText3, results[2].Text);
        }


        [DataTestMethod]
        [DataRow("/*abc*/", 0, 6, true)]
        [DataRow("/*abc", 0, 4, false)]
        [DataRow("/*--*/", 0, 5, true)]
        [DataRow("/*--", 0, 3, false)]
        [DataRow("/*  */", 0, 5, true)]
        [DataRow("/*  ", 0, 3, false)]
        [DataRow("/*/", 0, 2, false)]
        //[DataRow("/*/**/*/", 0, 7, true)] // FAILS!!! Embedded blocks scenario
        public void itShouldTokenizeBlockComments(string input,
                                               int expectedStartIndex,
                                               int expectedEndIndex,
                                               bool expectedIsTerminatedBlock) {
            var sut = new Tokenizer2();

            var result = sut.Tokenize(input);

            Assert.AreEqual(1, result.Count());
            var blockComment = result.First();
            Assert.AreEqual(input, blockComment.Text);
            Assert.AreEqual(expectedStartIndex, blockComment.StartIndex);
            Assert.AreEqual(expectedEndIndex, blockComment.EndIndex);
            Assert.AreEqual(TokenTypeEnum.BlockComment, blockComment.TokenType);
            Assert.AreEqual(expectedIsTerminatedBlock, blockComment.IsTerminatedBlock);
        }


        [DataTestMethod]
        [DataRow("/* abc ", "a*/", true)]
        [DataRow("/* abc ", "a --/", false)]
        public void itShouldNotTerminateBlockCommentsOnNewLine(string inputPart1, string inputPart2,
                                                                bool expectedIsTerminatedValue) {
            var input = $"{inputPart1}{Environment.NewLine}{inputPart2}";

            var sut = new Tokenizer2();

            var result = sut.Tokenize(input);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(TokenTypeEnum.BlockComment, result.First().TokenType);
            Assert.AreEqual(expectedIsTerminatedValue, result.First().IsTerminatedBlock);

        }


        [DataTestMethod]
        [DataRow("'a'", 1, true)]
        [DataRow("'a", 1, false)]
        [DataRow("' '", 1, true)]
        [DataRow("' ", 1, false)]
        [DataRow("'--'", 1, true)]
        [DataRow("'--", 1, false)]
        [DataRow("'/**/'", 1, true)]
        [DataRow("'/**/", 1, false)]
        [DataRow("'--abc--'", 1, true)]
        [DataRow("'--abc--", 1, false)]
        public void itShouldTokenizeStringLiterals(string input, 
                                                   int expectedTokenCount, 
                                                   bool expectedIsTerminatedBlockSetting) {

            var sut = new Tokenizer2();

            var result = sut.Tokenize(input);

            Assert.AreEqual(expectedTokenCount, result.Count());
            Assert.AreEqual(TokenTypeEnum.StringLiteral, result.First().TokenType);
            Assert.AreEqual(expectedIsTerminatedBlockSetting, result.First().IsTerminatedBlock);
        }
    }
}
