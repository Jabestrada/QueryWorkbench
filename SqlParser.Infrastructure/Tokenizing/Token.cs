using SqlParser.Infrastructure.CodeComments;
using System.Collections.Generic;

namespace SqlParser.Infrastructure.Tokenizing {
    public class Token {
        public int Index { get; protected set; }

        public IEnumerable<CodeCommentIndex> CodeCommentIndices { get; protected set; }

        //public IEnumerable<TokenPart> TokenParts { get; set; }
        public IEnumerable<Token> Tokens { get; set; }

        public int Length => Text.Length;

        public string Text { get; set; }
        public Token(string tokenizedWord, int startIndex, 
                         IEnumerable<CodeCommentIndex> codeCommentIndices) {
            Text = tokenizedWord;
            Index = startIndex;
            CodeCommentIndices = codeCommentIndices;
        }
    }
}
