using System;
using System.Runtime.CompilerServices;

namespace SqlParser.Infrastructure.Tokenizing2 {
    public enum TokenTypeEnum {
        NotSet,
        Whitespace,
        UnclassifiedIdentifier,
        StringLiteral,
        InlineComment,
        BlockComment
    }
    public static class TokenTypeEnumExtensions {
        public static bool IsBlockToken(this TokenTypeEnum tokenType) {
            return tokenType == TokenTypeEnum.BlockComment ||
                   tokenType == TokenTypeEnum.StringLiteral;
        }
    }
}
