namespace SqlParser.Infrastructure.Tokenizing2 {
    public class SqlCommentBlock : BaseBlockToken {
        public override TokenTypeEnum TokenType => TokenTypeEnum.BlockComment;

        public SqlCommentBlock(string startBlockToken, string endBlockToken, Token2 Token) : base(startBlockToken, endBlockToken, Token) {
        }
    }
}
