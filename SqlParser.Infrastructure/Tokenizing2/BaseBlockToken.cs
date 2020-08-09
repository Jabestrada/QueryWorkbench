using System.Collections.Generic;

namespace SqlParser.Infrastructure.Tokenizing2 {
    public abstract class BaseBlockToken {
        protected readonly Token2 Token;

        public BaseBlockToken(string startBlockToken, string endBlockToken, Token2 token) {
            StartBlockToken = startBlockToken;
            EndBlockToken = endBlockToken;
            Token = token;
        }

        public abstract TokenTypeEnum TokenType { get; }

        public string StartBlockToken { get; set; }
        public string EndBlockToken { get; set; }

        public virtual bool IsBlockStart(char ch, int charIndex,
                                             List<Token2> results, out bool wasCharAppended) {
            wasCharAppended = false;

            if (Token.TokenType.IsBlockToken()) return false;

            if (!Token.WillMatch(ch, StartBlockToken)) return false;

            Token.CreateTokenFromPrecedingCharsIfAny(charIndex, results);
          
            Token.Buffer = StartBlockToken;
            Token.StartIndex = charIndex - 1;
            Token.EndIndex = charIndex;
            wasCharAppended = true;
            Token.TokenType = TokenType;
            return true;
        }

        public virtual bool IsBlockContinuation(char ch, int charIndex,
                                      List<Token2> results, out bool wasCharAppended) {
            wasCharAppended = false;

            if (Token.TokenType != TokenType) return false;

            // _buffer.Length > 2 handles case where input == /*/
            var isTerminatingChar = Token.WillMatch(ch, EndBlockToken) && Token.Buffer.Length > StartBlockToken.Length;

            Token.Buffer += ch;
            Token.EndIndex = charIndex;
            wasCharAppended = true;

            if (isTerminatingChar) {
                Token.IsTerminatedBlock = true;
                Token.IsDone = true;
            }

            return true;
        }
    }
}
