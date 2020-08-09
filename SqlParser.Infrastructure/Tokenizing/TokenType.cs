namespace SqlParser.Infrastructure.Tokenizing {
    public enum TokenType {
        NotIdentified = 1,
        InlineComment = 2,
        BlockCommment = 4
    }

    public static class TokenTypeExtensions {
        public static bool HandlesComments(this TokenType t) {
            return (t | TokenType.BlockCommment) == 0 &&
                   (t | TokenType.InlineComment) == 0;
        }
    }
}
