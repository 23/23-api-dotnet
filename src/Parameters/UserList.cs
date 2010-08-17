namespace Twentythree
{
    public enum UserListSort
    {
        [RequestValue("username")]
        Username,
        [RequestValue("site_admin_p")]
        SiteAdmin,
        [RequestValue("email")]
        Email,
        [RequestValue("creation_date")]
        CreationDate,
        [RequestValue("last_login")]
        LastLogin,
        [RequestValue("display_name")]
        DisplayName
    }

    public class UserListParameters
    {
        public int? UserId = null;

        public string Search = null;

        public UserListSort OrderBy = UserListSort.DisplayName;
        public GenericSort Order = GenericSort.Descending;

        public int? PageOffset = null;
        public int? Size = null;
    }
}