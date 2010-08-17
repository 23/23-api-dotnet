using System;
using System.Collections.Generic;
using System.Web;
using System.Xml.XPath;
using DotNetOpenAuth.Messaging;

namespace Twentythree
{
    public interface IAlbumService
    {
        List<Domain.Album> GetList();
        List<Domain.Album> GetList(AlbumListParameters RequestParameters);
        int? Create(string Title);
        int? Create(string Title, string Description);
        int? Create(string Title, string Description, bool Hide);
        int? Create(string Title, string Description, bool Hide, int? UserId);
        bool Update(int AlbumId, string Title);
        bool Update(int AlbumId, string Title, string Description);
        bool Update(int AlbumId, string Title, string Description, bool Hide);
        bool Delete(int AlbumId);
    }

    public class AlbumService : IAlbumService
    {
        private IAPIProvider Provider;

        public AlbumService(IAPIProvider Provider)
        {
            this.Provider = Provider;
        }

        // * Get album list
        // Implements http://www.23developer.com/api/album-list
        /// <summary>
        /// Returns a list of albums by default parameters
        /// </summary>
        public List<Domain.Album> GetList()
        {
            return this.GetList(new AlbumListParameters());
        }

        /// <summary>
        /// Returns a list of albums by specific parameters
        /// </summary>
        public List<Domain.Album> GetList(AlbumListParameters RequestParameters)
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            if (RequestParameters.AlbumId != null) RequestURLParameters.Add("album_id=" + RequestParameters.AlbumId.ToString());
            if (RequestParameters.UserId != null) RequestURLParameters.Add("user_id=" + RequestParameters.UserId.ToString());
            if (RequestParameters.PhotoId != null) RequestURLParameters.Add("photo_id=" + RequestParameters.PhotoId.ToString());

            if (RequestParameters.Search != null) RequestURLParameters.Add("search=" + HttpUtility.UrlEncode(RequestParameters.Search));

            if (RequestParameters.IncludeHidden != false) RequestURLParameters.Add("include_hidden_p=1");

            if (RequestParameters.OrderBy != AlbumListSort.CreationDate) RequestURLParameters.Add("orderby=" + RequestValues.Get(RequestParameters.OrderBy));
            if (RequestParameters.Order != GenericSort.Descending) RequestURLParameters.Add("order=" + RequestValues.Get(RequestParameters.Order));

            if (RequestParameters.PageOffset != null) RequestURLParameters.Add("p=" + RequestParameters.PageOffset.ToString());
            if (RequestParameters.Size != null) RequestURLParameters.Add("size=" + RequestParameters.Size.ToString());

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/album/list", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return null;

            // List all the videos
            XPathNodeIterator Albums = ResponseMessage.Select("/response/album");
            List<Domain.Album> Result = new List<Domain.Album>();

            while (Albums.MoveNext())
            {
                // Create the domain album
                Domain.Album AlbumModel = new Domain.Album();

                AlbumModel.AlbumId = Helpers.ConvertStringToInteger(Albums.Current.GetAttribute("album_id", ""));

                AlbumModel.Title = Albums.Current.GetAttribute("title", "");

                AlbumModel.PrettyDate = Albums.Current.GetAttribute("pretty_date", "");
                AlbumModel.PrettyTime = Albums.Current.GetAttribute("pretty_time", "");

                AlbumModel.One = Albums.Current.GetAttribute("one", "");

                AlbumModel.CreationDateANSI = Albums.Current.GetAttribute("creation_date_ansi", "");

                AlbumModel.UserId = Helpers.ConvertStringToInteger(Albums.Current.GetAttribute("user_id", ""));
                AlbumModel.UserUrl = Albums.Current.GetAttribute("user_url", "");
                AlbumModel.Username = Albums.Current.GetAttribute("username", "");
                AlbumModel.DisplayName = Albums.Current.GetAttribute("display_name", "");

                AlbumModel.Token = Albums.Current.GetAttribute("token", "");

                AlbumModel.Hide = (Albums.Current.GetAttribute("hide_p", "") == "1");

                AlbumModel.Content = Helpers.GetNodeChildValue(Albums.Current, "content");
                AlbumModel.ContentText = Helpers.GetNodeChildValue(Albums.Current, "content_text");

                Result.Add(AlbumModel);
            }
            
            return Result;
        }

        // * Create album
        // Implements http://www.23developer.com/api/album-create
        /// <summary>Create an album with a specified title - returns album id or null</summary>
        public int? Create(string Title) { return Create(Title, "", false, null); }
        /// <summary>Create an album with a specified title and description - returns album id or null</summary>
        public int? Create(string Title, string Description) { return Create(Title, Description, false, null); }
        /// <summary>Create an album with a specified title, description and hiding - returns album id or null</summary>
        public int? Create(string Title, string Description, bool Hide) { return Create(Title, Description, Hide, null); }

        /// <summary>Create an album with a specified title, description, hiding and creator user id - returns album id or null</summary>
        public int? Create(string Title, string Description, bool Hide, int? UserId)
        {
            // Verify required parameters
            if (String.IsNullOrEmpty(Title)) return null;

            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            RequestURLParameters.Add("title=" + HttpUtility.UrlEncode(Title));
            if (!String.IsNullOrEmpty(Description)) RequestURLParameters.Add("description=" + HttpUtility.UrlEncode(Description));
            if (Hide) RequestURLParameters.Add("hide_p=1");
            if (UserId != null) RequestURLParameters.Add("user_id=" + UserId);

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/album/create", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return null;

            // Get the album id
            XPathNodeIterator Albums = ResponseMessage.Select("/response/album_id");
            if (Albums.MoveNext()) return Helpers.ConvertStringToInteger(Albums.Current.Value);
            
            // If nothing pops up, we'll return null
            return null;
        }

        // * Update album
        // Implements http://www.23developer.com/api/album-update
        /// <summary>Update an album given an id with a specified title</summary>
        public bool Update(int AlbumId, string Title) { return Update(AlbumId, Title, "", false); }
        /// <summary>Update an album given an id with a specified title and description</summary>
        public bool Update(int AlbumId, string Title, string Description) { return Update(AlbumId, Title, Description, false); }

        /// <summary>Update an album given an id with a specified title, description and hiding</summary>
        public bool Update(int AlbumId, string Title, string Description, bool Hide)
        {
            // Verify required parameters
            if (String.IsNullOrEmpty(Title)) return false;

            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            RequestURLParameters.Add("album_id=" + AlbumId.ToString());
            RequestURLParameters.Add("title=" + HttpUtility.UrlEncode(Title));
            if (!String.IsNullOrEmpty(Description)) RequestURLParameters.Add("description=" + HttpUtility.UrlEncode(Description));
            if (Hide) RequestURLParameters.Add("hide_p=1");

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/album/update", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return false;

            // If nothing pops up, we'll return true
            return true;
        }

        // * Delete album
        // Implements http://www.23developer.com/api/album-delete
        /// <summary>Delete an album given an id</summary>
        public bool Delete(int AlbumId)
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            RequestURLParameters.Add("album_id=" + AlbumId.ToString());

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/album/delete", RequestURLParameters), HttpDeliveryMethods.PostRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return false;

            // If nothing pops up, we'll return true
            return true;
        }
    }
}
