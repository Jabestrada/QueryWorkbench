using SqlParser.Infrastructure.CodeComments;
using System.Collections.Generic;

namespace SqlParser.Infrastructure.Tokenizing {
    public class SentenceTokenizer {
        public string NonBlockCommentToken {
            get; protected set;
        }
        public BlockCommentTokenDefinition BlockCommentToken { get; set; }

        public SentenceTokenizer(string nonBlockCommentToken = "--") {
            NonBlockCommentToken = nonBlockCommentToken;
        }

        public IEnumerable<Token> Tokenize(string sentence) {
            var results = new List<Token>();
            var tokenizer = newTokenizer();
            for (int k = 0; k < sentence.Length; k++) {
                if (tokenizer == null) {
                    tokenizer = newTokenizer();
                }
                if (char.IsWhiteSpace(sentence[k])) {
                    if (tokenizer.Length > 0) {
                        results.Add(tokenizer.BuildToken());
                    }
                    tokenizer = null;
                    continue;
                }
                else {
                    tokenizer.AcceptChar(sentence[k], k);
                }
                if (k + 1 == sentence.Length) {
                    if (tokenizer.Length > 0) {
                        results.Add(tokenizer.BuildToken());
                    }
                }
            }
            return results;
        }

        public SentenceTokenizer WithBlockCommentToken(BlockCommentTokenDefinition blockCommentTokenDef) {
            BlockCommentToken = blockCommentTokenDef;
            return this;
        }

        private WordTokenizer newTokenizer() {
            return new WordTokenizer(NonBlockCommentToken)
                            .WithBlockCommentToken(BlockCommentToken ?? new BlockCommentTokenDefinition("/*", "*/"));
        }
    }

}
