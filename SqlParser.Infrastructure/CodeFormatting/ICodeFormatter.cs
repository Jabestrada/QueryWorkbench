using SqlParser.Infrastructure.Tokenizing;

namespace SqlParser.Infrastructure.CodeFormatting {
    public interface ICodeDecorators {
        void Decorate(string word, int startIndex, int endIndex);
        TokenType PartTypes { get; }
    }

    public class NonCommentDecorator : ICodeDecorators {
        public TokenType PartTypes => TokenType.NotIdentified;

        public void Decorate(string word, int startIndex, int endIndex) {
        
        }
    }
}
