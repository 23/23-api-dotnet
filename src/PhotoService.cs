using System;
using System.Collections.Generic;
using System.Web;
using System.Xml.XPath;
using DotNetOpenAuth.Messaging;

namespace Twentythree
{
    public interface IPhotoService
    {
        List<Domain.Photo> GetList();
        List<Domain.Photo> GetList(PhotoListParameters RequestParameters);
        int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream);
        int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId);
        int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId);
        int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId, string Title);
        int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId, string Title, string Description);
        int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId, string Title, string Description, string Tags);
        int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId, string Title, string Description, string Tags, bool? Publish);
        bool Delete(int PhotoId);
        bool Replace(int PhotoId, string Filename, string FileContentType, System.IO.Stream Filestream);
        bool Update(int PhotoId, int? AlbumId);
        bool Update(int PhotoId, int? AlbumId, string Title);
        bool Update(int PhotoId, int? AlbumId, string Title, string Description);
        bool Update(int PhotoId, int? AlbumId, string Title, string Description, string Tags);
        bool Update(int PhotoId, int? AlbumId, string Title, string Description, string Tags, bool? Published);
        Domain.PhotoUploadToken GetUploadToken(string ReturnURL, bool? BackgroundReturn, int? UserId, int? AlbumId, string Title, string Description, string Tags, bool? Publish, int? ValidMinutes, int? MaxUploads);
        bool RedeemUploadToken(string Filename, string FileContentType, System.IO.Stream Filestream, string UploadToken);
    }

    public class PhotoService : IPhotoService
    {
        private IAPIProvider Provider;

        public PhotoService(IAPIProvider Provider)
        {
            this.Provider = Provider;
        }

        // * Get photo list
        // Implements http://www.23developer.com/api/photo-list
        /// <summary>
        /// Returns a list of photos by default parameters
        /// </summary>
        public List<Domain.Photo> GetList()
        {
            return this.GetList(new PhotoListParameters());
        }

        /// <summary>
        /// Returns a list of photos by specific parameters
        /// </summary>
        public List<Domain.Photo> GetList(PhotoListParameters RequestParameters)
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            if (RequestParameters.AlbumId != null) RequestURLParameters.Add("album_id=" + RequestParameters.AlbumId.ToString());
            if (RequestParameters.PhotoId != null) RequestURLParameters.Add("photo_id=" + RequestParameters.PhotoId.ToString());
            if (RequestParameters.UserId != null) RequestURLParameters.Add("user_id=" + RequestParameters.UserId.ToString());
            if (RequestParameters.PlayerId != null) RequestURLParameters.Add("player_id=" + RequestParameters.PlayerId.ToString());

            if (RequestParameters.Token != null) RequestURLParameters.Add("token=" + HttpUtility.UrlEncode(RequestParameters.Token));
            if (RequestParameters.Search != null) RequestURLParameters.Add("search=" + HttpUtility.UrlEncode(RequestParameters.Search));

            if (RequestParameters.Year != null) RequestURLParameters.Add("year=" + RequestParameters.Year.ToString());
            if (RequestParameters.Month != null) RequestURLParameters.Add("month=" + RequestParameters.Month.ToString());
            if (RequestParameters.Day != null) RequestURLParameters.Add("day=" + RequestParameters.Day.ToString());

            if (RequestParameters.Video != null) RequestURLParameters.Add("video_p=" + (RequestParameters.Video.Value ? "1" : "0"));
            if (RequestParameters.Audio != null) RequestURLParameters.Add("audio_p=" + (RequestParameters.Audio.Value ? "1" : "0"));
            if (RequestParameters.VideoEncoded != null) RequestURLParameters.Add("video_encoded_p=" + (RequestParameters.VideoEncoded.Value ? "1" : "0"));

            RequestURLParameters.Add("include_unpublished_p=" + (RequestParameters.IncludeUnpublised ? "1" : "0"));

            if (RequestParameters.Tags.Count > 0)
            {
                foreach (string _Tag in RequestParameters.Tags)
                {
                    RequestURLParameters.Add("tag=" + HttpUtility.UrlEncode(_Tag));
                }
            }
            if (RequestParameters.TagMode != PhotoTagMode.And) RequestURLParameters.Add("tag_mode=" + RequestValues.Get(RequestParameters.TagMode));

            if (RequestParameters.OrderBy != PhotoListSort.Published) RequestURLParameters.Add("orderby=" + RequestValues.Get(RequestParameters.OrderBy));
            if (RequestParameters.Order != GenericSort.Descending) RequestURLParameters.Add("order=" + RequestValues.Get(RequestParameters.Order));

            if (RequestParameters.PageOffset != null) RequestURLParameters.Add("p=" + RequestParameters.PageOffset.ToString());
            if (RequestParameters.Size != null) RequestURLParameters.Add("size=" + RequestParameters.Size.ToString());

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/photo/list", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return null;

            // List all the videos
            XPathNodeIterator Photos = ResponseMessage.Select("/response/photo");
            List<Domain.Photo> Result = new List<Domain.Photo>();

            while (Photos.MoveNext())
            {
                // Create the domain Photo
                Domain.Photo PhotoModel = new Domain.Photo()
                {
                    PhotoId = Helpers.ConvertStringToInteger(Photos.Current.GetAttribute("photo_id", "")),

                    Title = Photos.Current.GetAttribute("title", ""),
                    Token = Photos.Current.GetAttribute("token", ""),
                    One = Photos.Current.GetAttribute("one", ""),

                    Published = (Photos.Current.GetAttribute("published_p", "") == "1"),

                    CreationDateANSI = Photos.Current.GetAttribute("creation_date_ansi", ""),
                    CreationDateDate = Photos.Current.GetAttribute("creation_date__date", ""),
                    CreationDateTime = Photos.Current.GetAttribute("creation_date__time", ""),

                    OriginalDateANSI = Photos.Current.GetAttribute("original_date_ansi", ""),
                    OriginalDateDate = Photos.Current.GetAttribute("original_date__date", ""),
                    OriginalDateTime = Photos.Current.GetAttribute("original_date__time", ""),

                    ViewCount = Helpers.ConvertStringToInteger(Photos.Current.GetAttribute("view_count", "")),
                    NumberOfComments = Helpers.ConvertStringToInteger(Photos.Current.GetAttribute("number_of_comments", "")),
                    NumberOfAlbums = Helpers.ConvertStringToInteger(Photos.Current.GetAttribute("number_of_albums", "")),
                    NumberOfTags = Helpers.ConvertStringToInteger(Photos.Current.GetAttribute("number_of_tags", "")),
                    NumberOfRatings = Helpers.ConvertStringToInteger(Photos.Current.GetAttribute("number_of_ratings", "")),

                    PhotoRating = Helpers.ConvertStringToDouble(Photos.Current.GetAttribute("photo_rating", "")),

                    IsVideo = (Photos.Current.GetAttribute("video_p", "") == "1"),
                    IsAudio = (Photos.Current.GetAttribute("audio_p", "") == "1"),
                    VideoEncoded = (Photos.Current.GetAttribute("video_encoded_p", "") == "1"),
                    TextOnly = (Photos.Current.GetAttribute("text_only_p", "") == "1"),

                    VideoLength = Helpers.ConvertStringToDouble(Photos.Current.GetAttribute("video_length", "")),

                    UserId = Helpers.ConvertStringToInteger(Photos.Current.GetAttribute("user_id", "")),
                    Username = Photos.Current.GetAttribute("username", ""),
                    DisplayName = Photos.Current.GetAttribute("display_name", ""),
                    UserURL = Photos.Current.GetAttribute("user_url", ""),

                    Original = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("original_width", ""),
                        Photos.Current.GetAttribute("original_height", ""),
                        Photos.Current.GetAttribute("original_size", ""),
                        Photos.Current.GetAttribute("original_download", "")
                    ),

                    Quad16 = new Domain.PhotoBlock(
                        "16",
                        "16",
                        "0",
                        Photos.Current.GetAttribute("quad16_download", "")
                    ),
                    Quad50 = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("quad50_width", ""),
                        Photos.Current.GetAttribute("quad50_height", ""),
                        Photos.Current.GetAttribute("quad50_size", ""),
                        Photos.Current.GetAttribute("quad50_download", "")
                    ),
                    Quad75 = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("quad75_width", ""),
                        Photos.Current.GetAttribute("quad75_height", ""),
                        Photos.Current.GetAttribute("quad75_size", ""),
                        Photos.Current.GetAttribute("quad75_download", "")
                    ),
                    Quad100 = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("quad100_width", ""),
                        Photos.Current.GetAttribute("quad100_height", ""),
                        Photos.Current.GetAttribute("quad100_size", ""),
                        Photos.Current.GetAttribute("quad100_download", "")
                    ),

                    Small = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("small_width", ""),
                        Photos.Current.GetAttribute("small_height", ""),
                        Photos.Current.GetAttribute("small_size", ""),
                        Photos.Current.GetAttribute("small_download", "")
                    ),
                    Medium = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("medium_width", ""),
                        Photos.Current.GetAttribute("medium_height", ""),
                        Photos.Current.GetAttribute("medium_size", ""),
                        Photos.Current.GetAttribute("medium_download", "")
                    ),
                    Portrait = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("portrait_width", ""),
                        Photos.Current.GetAttribute("portrait_height", ""),
                        Photos.Current.GetAttribute("portrait_size", ""),
                        Photos.Current.GetAttribute("portrait_download", "")
                    ),
                    Standard = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("standard_width", ""),
                        Photos.Current.GetAttribute("standard_height", ""),
                        Photos.Current.GetAttribute("standard_size", ""),
                        Photos.Current.GetAttribute("standard_download", "")
                    ),
                    Large = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("large_width", ""),
                        Photos.Current.GetAttribute("large_height", ""),
                        Photos.Current.GetAttribute("large_size", ""),
                        Photos.Current.GetAttribute("large_download", "")
                    ),

                    VideoSmall = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("video_small_width", ""),
                        Photos.Current.GetAttribute("video_small_height", ""),
                        Photos.Current.GetAttribute("video_small_size", ""),
                        Photos.Current.GetAttribute("video_small_download", "")
                    ),
                    VideoMedium = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("video_medium_width", ""),
                        Photos.Current.GetAttribute("video_medium_height", ""),
                        Photos.Current.GetAttribute("video_medium_size", ""),
                        Photos.Current.GetAttribute("video_medium_download", "")
                    ),
                    VideoHD = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("video_hd_width", ""),
                        Photos.Current.GetAttribute("video_hd_height", ""),
                        Photos.Current.GetAttribute("video_hd_size", ""),
                        Photos.Current.GetAttribute("video_hd_download", "")
                    ),
                    VideoMobileH263AMR = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("video_mobile_h263_amr_width", ""),
                        Photos.Current.GetAttribute("video_mobile_h263_amr_height", ""),
                        Photos.Current.GetAttribute("video_mobile_h263_amr_size", ""),
                        Photos.Current.GetAttribute("video_mobile_h263_amr_download", "")
                    ),
                    VideoMobileH263AAC = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("video_mobile_h263_aac_width", ""),
                        Photos.Current.GetAttribute("video_mobile_h263_aac_height", ""),
                        Photos.Current.GetAttribute("video_mobile_h263_aac_size", ""),
                        Photos.Current.GetAttribute("video_mobile_h263_aac_download", "")
                    ),
                    VideoMobileMPEGE4AMR = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("video_mobile_mpeg4_amr_width", ""),
                        Photos.Current.GetAttribute("video_mobile_mpeg4_amr_height", ""),
                        Photos.Current.GetAttribute("video_mobile_mpeg4_amr_size", ""),
                        Photos.Current.GetAttribute("video_mobile_mpeg4_amr_download", "")
                    ),

                    Audio = new Domain.PhotoBlock(
                        Photos.Current.GetAttribute("audio_width", ""),
                        Photos.Current.GetAttribute("audio_height", ""),
                        Photos.Current.GetAttribute("audio_size", ""),
                        Photos.Current.GetAttribute("audio_download", "")
                    ),

                    Content = Helpers.GetNodeChildValue(Photos.Current, "content"),
                    ContentText = Helpers.GetNodeChildValue(Photos.Current, "content_text"),

                    BeforeDownloadType = Helpers.GetNodeChildValue(Photos.Current, "before_download_type"),
                    BeforeDownloadURL = Helpers.GetNodeChildValue(Photos.Current, "before_download_url"),
                    AfterDownloadType = Helpers.GetNodeChildValue(Photos.Current, "after_download_type"),
                    AfterDownloadURL = Helpers.GetNodeChildValue(Photos.Current, "after_download_url"),

                    AfterText = Helpers.GetNodeChildValue(Photos.Current, "after_text"),

                    Tags = new List<string>(Helpers.GetNodeChildValue(Photos.Current, "tags").Split(','))
                };

                Result.Add(PhotoModel);
            }
            
            return Result;
        }

        // * Upload photo
        // Implements http://www.23developer.com/api/photo-upload
        /// <summary>Upload a photo</summary>
        public int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream) { return Upload(Filename, FileContentType, Filestream, null, null, null, null, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId) { return Upload(Filename, FileContentType, Filestream, UserId, null, null, null, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId) { return Upload(Filename, FileContentType, Filestream, UserId, AlbumId, null, null, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId, string Title) { return Upload(Filename, FileContentType, Filestream, UserId, AlbumId, Title, null, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId, string Title, string Description) { return Upload(Filename, FileContentType, Filestream, UserId, AlbumId, Title, Description, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId, string Title, string Description, string Tags) { return Upload(Filename, FileContentType, Filestream, UserId, AlbumId, Title, Description, Tags, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string Filename, string FileContentType, System.IO.Stream Filestream, int? UserId, int? AlbumId, string Title, string Description, string Tags, bool? Publish)
        {
            // Verify required parameters
            if (Filestream == null) return null;

            // Build request URL
            List<MultipartPostPart> Data = new List<MultipartPostPart>()
            {
                MultipartPostPart.CreateFormFilePart("file", Filename, FileContentType, Filestream)
            };

            if (UserId != null) Data.Add(MultipartPostPart.CreateFormPart("user_id", UserId.ToString()));
            if (AlbumId != null) Data.Add(MultipartPostPart.CreateFormPart("album_id", AlbumId.ToString()));
            if (Title != null) Data.Add(MultipartPostPart.CreateFormPart("title", Title));
            if (Description != null) Data.Add(MultipartPostPart.CreateFormPart("description", Description));
            if (Tags != null) Data.Add(MultipartPostPart.CreateFormPart("tags", Tags));
            if (Publish != null) Data.Add(MultipartPostPart.CreateFormPart("publish", Publish.Value ? "1" : "0"));

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/photo/upload", null), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage, Data);
            if (ResponseMessage == null) return null;

            // Get the Photo id
            XPathNodeIterator Photos = ResponseMessage.Select("/response/photo_id");
            if (Photos.MoveNext()) return Helpers.ConvertStringToInteger(Photos.Current.Value);
            
            // If nothing pops up, we'll return null
            return null;
        }

        // * Delete photo
        // Implements http://www.23developer.com/api/photo-delete
        /// <summary>Delete a photo given an id</summary>
        public bool Delete(int PhotoId)
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            RequestURLParameters.Add("photo_id=" + PhotoId.ToString());

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/photo/delete", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return false;

            // If nothing pops up, we'll return true
            return true;
        }

        // * Replace photo
        // Implements http://www.23developer.com/api/photo-replace
        /// <summary>
        /// Replace a photo thumbnail given an id
        /// </summary>
        /// <param name="PhotoId">Id of photo</param>
        /// <param name="Filename">The original filename</param>
        /// <param name="FileContentType">The meta content type of the file</param>
        /// <param name="Filestream">An input stream for reading the file</param>
        /// <returns></returns>
        public bool Replace(int PhotoId, string Filename, string FileContentType, System.IO.Stream Filestream)
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/photo/replace", RequestURLParameters), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            List<MultipartPostPart> Data = new List<MultipartPostPart>()
            {
                MultipartPostPart.CreateFormPart("photo_id", PhotoId.ToString()),
                MultipartPostPart.CreateFormFilePart("file", Filename, FileContentType, Filestream)
            };

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage, Data);
            if (ResponseMessage == null) return false;

            // If nothing pops up, we'll return true
            return true;
        }

        // * Update photo
        // Implements
        /// <summary>Update a photo given the id</summary>
        public bool Update(int PhotoId, int? AlbumId) { return Update(PhotoId, AlbumId, null, null, null, null); }
        /// <summary>Update a photo given the id</summary>
        public bool Update(int PhotoId, int? AlbumId, string Title) { return Update(PhotoId, AlbumId, Title, null, null, null); }
        /// <summary>Update a photo given the id</summary>
        public bool Update(int PhotoId, int? AlbumId, string Title, string Description) { return Update(PhotoId, AlbumId, Title, Description, null, null); }
        /// <summary>Update a photo given the id</summary>
        public bool Update(int PhotoId, int? AlbumId, string Title, string Description, string Tags) { return Update(PhotoId, AlbumId, Title, Description, Tags, null); }
        /// <summary>Update a photo given the id</summary>
        public bool Update(int PhotoId, int? AlbumId, string Title, string Description, string Tags, bool? Published)
        {
            // Build request URL
            List<MultipartPostPart> Data = new List<MultipartPostPart>()
            {
                MultipartPostPart.CreateFormPart("photo_id", PhotoId.ToString())
            };

            if (AlbumId != null) Data.Add(MultipartPostPart.CreateFormPart("album_id", AlbumId.ToString()));
            if (Title != null) Data.Add(MultipartPostPart.CreateFormPart("title", Title));
            if (Description != null) Data.Add(MultipartPostPart.CreateFormPart("description", Description));
            if (Tags != null) Data.Add(MultipartPostPart.CreateFormPart("tags", Tags));
            if (Published != null) Data.Add(MultipartPostPart.CreateFormPart("publish", Published.Value ? "1" : "0"));

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/photo/update", null), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage, Data);
            if (ResponseMessage == null) return false;

            // If nothing pops up, we'll return null
            return true;
        }

        // * Get upload token
        // Implements http://www.23developer.com/api/photo-get-upload-token
        public Domain.PhotoUploadToken GetUploadToken(string ReturnURL, bool? BackgroundReturn, int? UserId, int? AlbumId, string Title, string Description, string Tags, bool? Publish, int? ValidMinutes, int? MaxUploads)
        {
            if (String.IsNullOrEmpty(ReturnURL)) return null;

            // Build request URL
            List<MultipartPostPart> Data = new List<MultipartPostPart>()
            {
                MultipartPostPart.CreateFormPart("return_url", ReturnURL)
            };

            if (BackgroundReturn != null) Data.Add(MultipartPostPart.CreateFormPart("background_return_p", BackgroundReturn.Value ? "1" : "0"));
            
            if (UserId != null) Data.Add(MultipartPostPart.CreateFormPart("user_id", UserId.ToString()));
            if (AlbumId != null) Data.Add(MultipartPostPart.CreateFormPart("album_id", AlbumId.ToString()));
            if (Title != null) Data.Add(MultipartPostPart.CreateFormPart("title", Title));
            if (Description != null) Data.Add(MultipartPostPart.CreateFormPart("description", Description));
            if (Tags != null) Data.Add(MultipartPostPart.CreateFormPart("tags", Tags));

            if (Publish != null) Data.Add(MultipartPostPart.CreateFormPart("publish", Publish.Value ? "1" : "0"));

            if (ValidMinutes != null) Data.Add(MultipartPostPart.CreateFormPart("valid_minutes", ValidMinutes.ToString()));
            if (MaxUploads != null) Data.Add(MultipartPostPart.CreateFormPart("max_uploads", MaxUploads.ToString()));

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/photo/get-upload-token", null), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage, Data);
            if (ResponseMessage == null) return null;

            // Get the token
            XPathNodeIterator TokenNode = ResponseMessage.Select("/response");
            if (!TokenNode.MoveNext()) return null;

            Domain.PhotoUploadToken UploadToken = new Domain.PhotoUploadToken()
            {
                UploadToken = Helpers.GetNodeChildValue(TokenNode.Current, "upload_token"),

                Title = Helpers.GetNodeChildValue(TokenNode.Current, "title"),
                Description = Helpers.GetNodeChildValue(TokenNode.Current, "description"),
                Tags = Helpers.GetNodeChildValue(TokenNode.Current, "tags"),

                Publish = (Helpers.GetNodeChildValue(TokenNode.Current, "publish") == "1"),

                UserId = Helpers.ConvertStringToInteger(Helpers.GetNodeChildValue(TokenNode.Current, "user_id")),
                AlbumId = Helpers.ConvertStringToInteger(Helpers.GetNodeChildValue(TokenNode.Current, "album_id")),

                ValidMinutes = Helpers.ConvertStringToInteger(Helpers.GetNodeChildValue(TokenNode.Current, "valid_minutes")),
                ValidUntil = Helpers.ConvertStringToInteger(Helpers.GetNodeChildValue(TokenNode.Current, "valid_until")),

                ReturnURL = Helpers.GetNodeChildValue(TokenNode.Current, "return_url")
            };

            // If nothing pops up, we'll return null
            return UploadToken;
        }

        // * Redeem upload token
        // Implements http://www.23developer.com/api/photo-redeem-upload-token
        public bool RedeemUploadToken(string Filename, string FileContentType, System.IO.Stream Filestream, string UploadToken)
        {
            // Verify required parameters
            if (Filestream == null) return false;

            // Build request URL
            List<MultipartPostPart> Data = new List<MultipartPostPart>()
            {
                MultipartPostPart.CreateFormFilePart("file", Filename, FileContentType, Filestream),
                MultipartPostPart.CreateFormPart("upload_token", UploadToken)
            };

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/photo/redeem-upload-token", null), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage, Data);
            if (ResponseMessage == null) return false;
            
            // If nothing pops up, we'll return null
            return true;
        }
    }
}