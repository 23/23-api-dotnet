namespace Twentythree
{
    public class CommentListParameters
    {
        public int? ObjectId = null;
        public CommentObjectType ObjectType = CommentObjectType.Empty;

        public int? CommentId = null;
        public int? CommentUserId = null;

        public string Search = null;

        public GenericSort Order = GenericSort.Descending;

        public int? PageOffset = null;
        public int? Size = null;
    }
}