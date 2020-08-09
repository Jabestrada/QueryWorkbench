using SqlParser.Infrastructure.CodeComments;
using SqlParser.Infrastructure.Tokenizing;
using System.Collections.Generic;
using System.Linq;

namespace SqlParser.Infrastructure.CodeFormatting {
    public class CodeParser {
        private CodeCommentIndex _unterminatedCodeCommentIndex;
        private IEnumerable<ICodeDecorators> _codeDecorators;

        public CodeParser(IEnumerable<ICodeDecorators> codeDecorators) {
            _codeDecorators = codeDecorators;
        }

        public void ApplyStyles(List<Token> tokens,
                                CodeCommentIndex unterminatedCodeCommentIndex = null) {

            _unterminatedCodeCommentIndex = unterminatedCodeCommentIndex;

            foreach (var token in tokens) {
                ApplyStyles(token);
            }
        }

        private void ApplyStyles(Token token) {
            if (!token.CodeCommentIndices.Any() && _unterminatedCodeCommentIndex == null) {
                decorateNonComment(token.Text, token.Index, token.Index + token.Length);
                return;
            }

            var currentCharPos = 0;
            foreach (var codeCommentIndex in token.CodeCommentIndices) {
                if (_unterminatedCodeCommentIndex == null) {
                    var subStringEndPos = codeCommentIndex.StartIndex.HasValue ?
                                          codeCommentIndex.StartIndex.Value :
                                          codeCommentIndex.StartOfEndIndex.Value;

                    var nonComment = token.Text.Substring(currentCharPos, subStringEndPos);
                    decorateNonComment(nonComment, currentCharPos, subStringEndPos);

                    if (codeCommentIndex.CommentType == CodeCommentType.Inline
                        || (codeCommentIndex.CommentType == CodeCommentType.Block &&
                            codeCommentIndex.StartIndex.HasValue)) {
                        _unterminatedCodeCommentIndex = codeCommentIndex;
                    }
                    currentCharPos += subStringEndPos;
                }
                else { 
                    // TODO: If comment is inline, ignore currentCodeCommentIndex EXCEPT if the latter
                    // is CodeBlockComment and StartIndex.HasValue
                    // START CREATING CODEPARSER TESTS!!!
                }
            }
        }

        private void decorateNonComment(string text, int startIndex, int endIndex) {
            foreach (var nonCommentDecorator in _codeDecorators.Where(d => !d.PartTypes.HandlesComments())) {
                nonCommentDecorator.Decorate(text, startIndex, endIndex);
            }
        }

        private bool unterminatedCommentIsStartBlock() {
            return _unterminatedCodeCommentIndex != null &&
                     _unterminatedCodeCommentIndex.CommentType == CodeCommentType.Block &&
                     _unterminatedCodeCommentIndex.StartIndex.HasValue;
        }
    }
}
