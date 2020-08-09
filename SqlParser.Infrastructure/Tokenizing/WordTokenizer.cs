using SqlParser.Infrastructure.CodeComments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlParser.Infrastructure.Tokenizing {
    public class WordTokenizer {
        private int _startIndex = -1;

        private TokenMatcher _nonBlockCommentTokenMatcher;
        private TokenMatcher _blockCommentStartMatcher;
        private TokenMatcher _blockCommentEndMatcher;

        private List<Func<char, int, bool>> _tokenMatchers = new List<Func<char, int, bool>>();

        private Stack<int> _blockCommentStartIndices = new Stack<int>();
        private List<BlockCommentIndex> _blockCommentIndices = new List<BlockCommentIndex>();

        private Lazy<StringBuilder> _lazyBuffer = new Lazy<StringBuilder>(() => new StringBuilder());
        protected StringBuilder Buffer => _lazyBuffer.Value;

        public int Length => Buffer.Length;

        public string NonBlockCommentToken { get; protected set; }
        public BlockCommentTokenDefinition BlockCommentToken { get; protected set; }

        public WordTokenizer(string nonBlockCommentToken = "--") {
            NonBlockCommentToken = nonBlockCommentToken;
            _nonBlockCommentTokenMatcher = new TokenMatcher(nonBlockCommentToken);

            BlockCommentToken = new BlockCommentTokenDefinition("/*", "*/");
            _blockCommentStartMatcher = new TokenMatcher(BlockCommentToken.StartBlockToken);
            _blockCommentEndMatcher = new TokenMatcher(BlockCommentToken.EndBlockToken);

            initializeTokenMatchers();
        }

        public WordTokenizer WithBlockCommentToken(BlockCommentTokenDefinition blockCommentTokenDef) {
            BlockCommentToken = blockCommentTokenDef;
            _blockCommentStartMatcher = new TokenMatcher(blockCommentTokenDef.StartBlockToken);
            _blockCommentEndMatcher = new TokenMatcher(blockCommentTokenDef.EndBlockToken);
            return this;
        }

        public void AcceptChar(char currentChar, int charIndex) {
            if (_startIndex == -1) {
                _startIndex = charIndex;
            }

            foreach (var tokenMatcher in _tokenMatchers) {
                tokenMatcher(currentChar, charIndex);
            }

            Buffer.Append(currentChar);
        }

        public Token BuildToken() {
            IEnumerable<CodeCommentIndex> codeCommentIndices = consolidateCodeCommentIndices();
            return new Token(Buffer.ToString(), _startIndex, codeCommentIndices);
        }

        #region non-public

        #region Token matcher delegates
        private void initializeTokenMatchers() {
            _tokenMatchers.AddRange(new Func<char, int, bool>[] {
                _nonBlockCommentTokenMatcher.TryMatchChar,
                tryMatchBlockCommmentStart,
                tryMatchBlockCommmentEnd
            });
        }

        private bool tryMatchBlockCommmentStart(char currentChar, int charIndex) {
            var matched = _blockCommentStartMatcher.TryMatchChar(currentChar, charIndex);
            if (matched) {
                _blockCommentStartIndices.Push(charIndex - (BlockCommentToken.StartBlockToken.Length - 1));
            }
            return matched;
        }

        private bool tryMatchBlockCommmentEnd(char currentChar, int charIndex) {
            if (!_blockCommentEndMatcher.TryMatchChar(currentChar, charIndex)) return false;
            
            if (_blockCommentStartIndices.Count == 0) {
                // End commend block has no matching start.
                _blockCommentIndices.Add(new BlockCommentIndex {
                    StartIndex = null,
                    EndIndex = charIndex
                });
            }
            else {
                // End block has a start block match.
                _blockCommentIndices.Add(new BlockCommentIndex {
                    StartIndex = _blockCommentStartIndices.Pop(),
                    EndIndex = charIndex
                });
            }
            return true;
        }

        #endregion

        private IEnumerable<CodeCommentIndex> consolidateCodeCommentIndices() {
            var codeCommentIndices = new List<CodeCommentIndex>();

            // Add inline comments to collection
            foreach (var index in _nonBlockCommentTokenMatcher.GetMatchedIndices()) {
                codeCommentIndices.Add(new CodeCommentIndex {
                    CommentType = CodeCommentType.Inline,
                    StartCommentToken = NonBlockCommentToken,
                    StartIndex = index
                });
            }

            // Add block comments to collection
            // First, account for remaining items in blockCommentStart, which are those 
            // that have no matching end tokens
            while (_blockCommentStartIndices.Count > 0) {
                codeCommentIndices.Add(new CodeCommentIndex {
                    CommentType = CodeCommentType.Block,
                    StartCommentToken = BlockCommentToken.StartBlockToken,
                    EndCommentToken = BlockCommentToken.EndBlockToken,
                    StartIndex = _blockCommentStartIndices.Pop(),
                    EndIndex = null
                });
            }

            // Then, add the block comments that have matching end tokens.
            codeCommentIndices.AddRange(_blockCommentIndices.Select(bci => new CodeCommentIndex {
                CommentType = CodeCommentType.Block,
                StartCommentToken = BlockCommentToken.StartBlockToken,
                EndCommentToken = BlockCommentToken.EndBlockToken,
                StartIndex = bci.StartIndex,
                EndIndex = bci.EndIndex
            }));

            // Return all comment indices
            return codeCommentIndices.OrderBy(ci => ci.StartIndex);
        }

        #endregion

    }
}
