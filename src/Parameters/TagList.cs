namespace Visual
{
    public enum TagListSort
    {
        [RequestValue("tag")]
        Tag,
        [RequestValue("count")]
        Count
    }

    public class TagListParameters
    {
        public string Search = null;

        public bool ReformatTags = false;
        public bool ExcludeMachineTags = false;

        public TagListSort OrderBy = TagListSort.Tag;
        public GenericSort Order = GenericSort.Descending;

        public int? PageOffset = null;
        public int? Size = null;
    }
}