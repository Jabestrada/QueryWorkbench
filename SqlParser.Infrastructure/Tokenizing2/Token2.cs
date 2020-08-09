using System;
using System.Collections.Generic;

namespace SqlParser.Infrastructure.Tokenizing2 {
    public class Token2 {
        public string Text { get; set; }
        public TokenTypeEnum TokenType { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public bool IsTerminatedBlock { get; set; }
        public string StartTokenText { get; set; }
        public string EndTokenText { get; set; }
        public IEnumerable<Token2> Tokens { get; set; }

        public string Buffer { get; set; } = string.Empty;

        public bool IsEmpty {
            get {
                return Buffer == null || Buffer.Length == 0;
            }
        }

        public bool IsDone { get; set; }

        private SqlCommentBlock _sqlBlockComment;
        private StringLiteralBlock _stringLiteralBlock;

        public Token2() {
            _sqlBlockComment = new SqlCommentBlock("/*", "*/", this);
            _stringLiteralBlock = new StringLiteralBlock("'", "'", this);
            StartIndex = -1;
            EndIndex = -1;
            initializeCharHandlers();
        }


        public bool TryAppend(char ch, int charIndex, List<Token2> results) {
            bool handlerResult;
            foreach (var charHandler in _charHandlers) {
                var charHandled = charHandler(ch, charIndex, results, out handlerResult);
                if (charHandled) {
                    return handlerResult;
                }
            }
            throw new InvalidOperationException("Missing TryAppend handler!!!");
        }


        public Token2 Finalize() {
            Text = Buffer;
            Buffer = string.Empty;

            if (TokenType != TokenTypeEnum.NotSet) return this;

            if (string.IsNullOrWhiteSpace(Text)) {
                TokenType = TokenTypeEnum.Whitespace;
            }
            else if (TokenType == TokenTypeEnum.BlockComment) {
                IsTerminatedBlock = Buffer.EndsWith(_sqlBlockComment.EndBlockToken);
            }
            //else {
            //    TokenType = TokenTypeEnum.UnclassifiedIdentifier;
            //}
            return this;
        }

        public bool WillMatch(char c, string matchWhat) {
            // Current buffer not existing or too small.
            if (Buffer == null || (Buffer.Length + 1) < matchWhat.Length) return false;

            // Given _buffer: '-' and matchWhat = '--', willMatch == true
            // Given _buffer: '*' and matchWhat = '*/', willMatch == true
            // Given _buffer: '--' and matchWhat = '-', willMatch == false
            // Given _buffer: '*/' and matchWhat = '*/', willMatch == false
            return Buffer.Substring(Buffer.Length - (matchWhat.Length - 1)) + c.ToString() == matchWhat;
        }

        #region Char handlers
        private delegate bool CharHandler(char ch, int charIndex, List<Token2> results, out bool wasCharAppended);
        private List<CharHandler> _charHandlers = new List<CharHandler>();

        private void initializeCharHandlers() {
            // CAUTION: Ordering of char handlers matter!!!
            _charHandlers.Add(isStartOfNewToken);
            _charHandlers.Add(isWhitespaceContinuation);
            _charHandlers.Add(isInlineCommentStart);
            _charHandlers.Add(_sqlBlockComment.IsBlockStart);
            _charHandlers.Add(_stringLiteralBlock.IsBlockStart);
            _charHandlers.Add(isInlineCommentContinuation);
            _charHandlers.Add(_sqlBlockComment.IsBlockContinuation);
            _charHandlers.Add(_stringLiteralBlock.IsBlockContinuation);
            _charHandlers.Add(isNonWhitespaceTermination);

            _charHandlers.Add(nonWhitespaceContinuation);
        }

        private bool isWhitespaceContinuation(char ch, int charIndex, List<Token2> results, out bool wasCharAppended) {
            wasCharAppended = false;
            if (!string.IsNullOrWhiteSpace(Buffer)) return false;

            if (char.IsWhiteSpace(ch)) {
                if (TokenType == TokenTypeEnum.NotSet) {
                    TokenType = TokenTypeEnum.Whitespace;
                }
                // Continue buffering stream of whitespaces.
                Buffer += ch;
                EndIndex = charIndex;
                wasCharAppended = true;
            }
            else {
                // Stream of whitespaces has ended so terminate this token.
                EndIndex = charIndex - 1;
            }
            return true;
        }

        private bool isInlineCommentContinuation(char ch, int charIndex, List<Token2> results, out bool wasCharAppended) {
            wasCharAppended = false;

            if (TokenType != TokenTypeEnum.InlineComment) return false;

            if (WillMatch(ch, Environment.NewLine)) {
                EndIndex = charIndex - 1;
            }
            else {
                Buffer += ch;
                EndIndex = charIndex;
                wasCharAppended = true;
            }
            return true;
        }

        private bool isStartOfNewToken(char ch, int charIndex, List<Token2> results, out bool wasCharAppended) {
            wasCharAppended = false;
            if (Buffer.Length > 0) return false;

            StartIndex = charIndex;
            EndIndex = charIndex;
            Buffer += ch;
            wasCharAppended = true;
            // Compare for single-char, block tokens; we only have 1 now but convert to list once another
            // is added in the future.
            if (ch.ToString() == _stringLiteralBlock.StartBlockToken) {
                TokenType = TokenTypeEnum.StringLiteral;
            }
            return true;
        }

        private bool isNonWhitespaceTermination(char ch, int charIndex, List<Token2> results, out bool wasCharAppended) {
            wasCharAppended = false;
            if (char.IsWhiteSpace(ch)) {
                // Stream of non-whitespaces has ended so terminate this token.
                EndIndex = charIndex - 1;
                return true;
            }
            return false;
        }

        private bool isTokenTypeSet() {
            return TokenType != TokenTypeEnum.NotSet;
        }


        private bool isInlineCommentStart(char ch, int charIndex, List<Token2> results, out bool wasCharAppended) {
            wasCharAppended = false;

            if (isTokenTypeSet()) {
                return false;
            }

            if (!WillMatch(ch, "--")) return false;

            CreateTokenFromPrecedingCharsIfAny(charIndex, results);

            Buffer = "--";
            StartIndex = charIndex - 1;
            EndIndex = charIndex;
            TokenType = TokenTypeEnum.InlineComment;
            wasCharAppended = true;
            return true;
        }

        public void CreateTokenFromPrecedingCharsIfAny(int charIndex, List<Token2> results) {
            if (Buffer.Length - 1 <= 1) return;

            var newToken = new Token2();
            newToken.Text = Buffer.Substring(0, Buffer.Length - 1);
            newToken.StartIndex = StartIndex;
            newToken.EndIndex = charIndex - 1;
            if (TokenType == TokenTypeEnum.NotSet) {
                newToken.TokenType = TokenTypeEnum.UnclassifiedIdentifier;
            }
            results.Add(newToken);
        }

        private bool nonWhitespaceContinuation(char ch, int charIndex, List<Token2> results, out bool wasCharAppended) {
            wasCharAppended = true;
            Buffer += ch;
            EndIndex = charIndex;
            return true;
        }
        #endregion Char handlers



    }
}
