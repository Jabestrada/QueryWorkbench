namespace SqlParser.Infrastructure.Tokenizing2 {
    public class StringLiteralBlock : BaseBlockToken {
        public StringLiteralBlock(string startBlockToken, string endBlockToken, Token2 token) : base(startBlockToken, endBlockToken, token) {
        }

        public override TokenTypeEnum TokenType => TokenTypeEnum.StringLiteral;

    }
}
