namespace SqlParser.Infrastructure.CodeComments {
    public class BlockCommentTokenDefinition {
        public readonly string StartBlockToken;
        public readonly string EndBlockToken;
        public BlockCommentTokenDefinition(string startBlockToken = "/*", string endBlockToken = "*/") {
            StartBlockToken = startBlockToken;
            EndBlockToken = endBlockToken;
        }
    }
}
