namespace Visual
{
    public enum AlbumListSort
    {
        [RequestValue("sortkey")]
        SortKey,
        [RequestValue("title")]
        Title,
        [RequestValue("editing_date")]
        EditingDate,
        [RequestValue("creation_date")]
        CreationDate
    }

    public class AlbumListParameters
    {
        public int? AlbumId = null;
        public int? UserId = null;
        public int? PhotoId = null;

        public string Search = null;

        public bool IncludeHidden = false;

        public AlbumListSort OrderBy = AlbumListSort.CreationDate;
        public GenericSort Order = GenericSort.Descending;

        public int? PageOffset = null;
        public int? Size = null;
    }
}