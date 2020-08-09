namespace SqlParser.Infrastructure.CodeComments {
    public class CodeCommentIndex {
        public string StartCommentToken { get; set; }
        public string EndCommentToken { get; set; }
        public int? StartIndex { get; set; }
        public int? EndIndex { get; set; }
        public int? StartOfEndIndex {
            get {
                if (!EndIndex.HasValue) return null;

                return EndIndex.Value - (EndCommentToken.Length - 1);
            }
        }
        public CodeCommentType CommentType { get; set; }
    }

    public enum CodeCommentType {
        Inline,
        Block
    }
}
