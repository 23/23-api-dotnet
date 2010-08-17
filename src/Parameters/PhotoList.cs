using System.Collections.Generic;
namespace Twentythree
{
    public enum PhotoListSort
    {
        [RequestValue("views")]
        Views,
        [RequestValue("comments")]
        Comments,
        [RequestValue("taken")]
        Taken,
        [RequestValue("title")]
        Title,
        [RequestValue("words")]
        Words,
        [RequestValue("rating")]
        Rating,
        [RequestValue("created")]
        Created,
        [RequestValue("uploaded")]
        Uploaded,
        [RequestValue("published")]
        Published
    }

    public enum PhotoTagMode
    {
        [RequestValue("any")]
        Any,
        [RequestValue("and")]
        And
    }

    public class PhotoListParameters
    {
        public int? AlbumId = null;
        public int? PhotoId = null;
        public int? UserId = null;
        public int? PlayerId = null;

        public string Token = null;

        public List<string> Tags = new List<string>();
        public PhotoTagMode TagMode = PhotoTagMode.And;

        public string Search = null;

        public int? Year = null;
        public int? Month = null;
        public int? Day = null;

        public bool? Video = null;
        public bool? Audio = null;

        public bool? VideoEncoded = null;
        public bool IncludeUnpublised = false;

        public PhotoListSort OrderBy = PhotoListSort.Published;
        public GenericSort Order = GenericSort.Descending;

        public int? PageOffset = null;
        public int? Size = null;
    }
}