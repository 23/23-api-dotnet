using System;
using System.Collections.Generic;
using System.Web;
using System.Xml.XPath;
using DotNetOpenAuth.Messaging;

namespace Visual
{
    public class PhotoService : IPhotoService
    {
        private IApiProvider _provider;

        public PhotoService(IApiProvider provider)
        {
            _provider = provider;
        }

        // * Get a single photo
        /// <summary>
        /// Returns a single photo object defined by the photo ID. Only includes published photos
        /// </summary>
        public Domain.Photo Get(int photoId) { return Get(photoId, false); }

        // * Get a single photo
        /// <summary>
        /// Returns a single photo object defined by the photo ID defined by inclusion of unpublished photos
        /// </summary>
        public Domain.Photo Get(int photoId, bool includeUnpublished)
        {
            // Get a list of photos
            List<Domain.Photo> photoList = GetList(new PhotoListParameters
            {
                PhotoId = photoId,
                IncludeUnpublished = includeUnpublished
            });

            // Verify photo list
            if ((photoList == null) || (photoList.Count == 0)) return null;

            // Return the first entry
            return photoList[0];
        }

        // * Get photo list
        // Implements http://www.23developer.com/api/photo-list
        /// <summary>
        /// Returns a list of photos by default parameters
        /// </summary>
        public List<Domain.Photo> GetList()
        {
            return GetList(new PhotoListParameters());
        }

        /// <summary>
        /// Returns a list of photos by specific parameters
        /// </summary>
        public List<Domain.Photo> GetList(PhotoListParameters requestParameters)
        {
            // Build request URL
            List<string> requestUrlParameters = new List<string>();

            if (requestParameters.AlbumId != null) requestUrlParameters.Add("album_id=" + requestParameters.AlbumId);
            if (requestParameters.PhotoId != null) requestUrlParameters.Add("photo_id=" + requestParameters.PhotoId);
            if (requestParameters.UserId != null) requestUrlParameters.Add("user_id=" + requestParameters.UserId);
            if (requestParameters.PlayerId != null) requestUrlParameters.Add("player_id=" + requestParameters.PlayerId);

            if (requestParameters.Token != null) requestUrlParameters.Add("token=" + HttpUtility.UrlEncode(requestParameters.Token));
            if (requestParameters.Search != null) requestUrlParameters.Add("search=" + HttpUtility.UrlEncode(requestParameters.Search));

            if (requestParameters.Year != null) requestUrlParameters.Add("year=" + requestParameters.Year);
            if (requestParameters.Month != null) requestUrlParameters.Add("month=" + requestParameters.Month);
            if (requestParameters.Day != null) requestUrlParameters.Add("day=" + requestParameters.Day);

            if (requestParameters.Video != null) requestUrlParameters.Add("video_p=" + (requestParameters.Video.Value ? "1" : "0"));
            if (requestParameters.Audio != null) requestUrlParameters.Add("audio_p=" + (requestParameters.Audio.Value ? "1" : "0"));
            if (requestParameters.VideoEncoded != null) requestUrlParameters.Add("video_encoded_p=" + (requestParameters.VideoEncoded.Value ? "1" : "0"));

            requestUrlParameters.Add("include_unpublished_p=" + (requestParameters.IncludeUnpublished ? "1" : "0"));

            if (requestParameters.Tags.Count > 0)
            {
                foreach (string tag in requestParameters.Tags)
                {
                    requestUrlParameters.Add("tag=" + HttpUtility.UrlEncode(tag));
                }
            }
            if (requestParameters.TagMode != PhotoTagMode.And) requestUrlParameters.Add("tag_mode=" + RequestValues.Get(requestParameters.TagMode));

            if (requestParameters.OrderBy != PhotoListSort.Published) requestUrlParameters.Add("orderby=" + RequestValues.Get(requestParameters.OrderBy));
            if (requestParameters.Order != GenericSort.Descending) requestUrlParameters.Add("order=" + RequestValues.Get(requestParameters.Order));

            if (requestParameters.PageOffset != null) requestUrlParameters.Add("p=" + requestParameters.PageOffset);
            if (requestParameters.Size != null) requestUrlParameters.Add("size=" + requestParameters.Size);

            // Do the request
            MessageReceivingEndpoint requestMessage = new MessageReceivingEndpoint(_provider.GetRequestUrl("/api/photo/list", requestUrlParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator responseMessage = _provider.DoRequest(requestMessage);
            if (responseMessage == null) return null;

            // List all the videos
            XPathNodeIterator photos = responseMessage.Select("/response/photo");
            List<Domain.Photo> result = new List<Domain.Photo>();

            while (photos.MoveNext())
            {
                // Create the domain Photo
                Domain.Photo photoModel = new Domain.Photo
                                              {
                    PhotoId = Helpers.ConvertStringToInteger(photos.Current.GetAttribute("photo_id", "")),

                    Title = photos.Current.GetAttribute("title", ""),
                    Token = photos.Current.GetAttribute("token", ""),
                    One = photos.Current.GetAttribute("one", ""),

                    Published = (photos.Current.GetAttribute("published_p", "") == "1"),

                    CreationDateANSI = photos.Current.GetAttribute("creation_date_ansi", ""),
                    CreationDateDate = photos.Current.GetAttribute("creation_date__date", ""),
                    CreationDateTime = photos.Current.GetAttribute("creation_date__time", ""),

                    OriginalDateANSI = photos.Current.GetAttribute("original_date_ansi", ""),
                    OriginalDateDate = photos.Current.GetAttribute("original_date__date", ""),
                    OriginalDateTime = photos.Current.GetAttribute("original_date__time", ""),

                    ViewCount = Helpers.ConvertStringToInteger(photos.Current.GetAttribute("view_count", "")),
                    NumberOfComments = Helpers.ConvertStringToInteger(photos.Current.GetAttribute("number_of_comments", "")),
                    NumberOfAlbums = Helpers.ConvertStringToInteger(photos.Current.GetAttribute("number_of_albums", "")),
                    NumberOfTags = Helpers.ConvertStringToInteger(photos.Current.GetAttribute("number_of_tags", "")),
                    NumberOfRatings = Helpers.ConvertStringToInteger(photos.Current.GetAttribute("number_of_ratings", "")),

                    PhotoRating = Helpers.ConvertStringToDouble(photos.Current.GetAttribute("photo_rating", "")),

                    IsVideo = (photos.Current.GetAttribute("video_p", "") == "1"),
                    IsAudio = (photos.Current.GetAttribute("audio_p", "") == "1"),
                    VideoEncoded = (photos.Current.GetAttribute("video_encoded_p", "") == "1"),
                    TextOnly = (photos.Current.GetAttribute("text_only_p", "") == "1"),

                    VideoLength = Helpers.ConvertStringToDouble(photos.Current.GetAttribute("video_length", "")),

                    UserId = Helpers.ConvertStringToInteger(photos.Current.GetAttribute("user_id", "")),
                    Username = photos.Current.GetAttribute("username", ""),
                    DisplayName = photos.Current.GetAttribute("display_name", ""),
                    UserURL = photos.Current.GetAttribute("user_url", ""),

                    Original = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("original_width", ""),
                        photos.Current.GetAttribute("original_height", ""),
                        photos.Current.GetAttribute("original_size", ""),
                        photos.Current.GetAttribute("original_download", "")
                    ),

                    Quad16 = new Domain.PhotoBlock(
                        "16",
                        "16",
                        "0",
                        photos.Current.GetAttribute("quad16_download", "")
                    ),
                    Quad50 = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("quad50_width", ""),
                        photos.Current.GetAttribute("quad50_height", ""),
                        photos.Current.GetAttribute("quad50_size", ""),
                        photos.Current.GetAttribute("quad50_download", "")
                    ),
                    Quad75 = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("quad75_width", ""),
                        photos.Current.GetAttribute("quad75_height", ""),
                        photos.Current.GetAttribute("quad75_size", ""),
                        photos.Current.GetAttribute("quad75_download", "")
                    ),
                    Quad100 = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("quad100_width", ""),
                        photos.Current.GetAttribute("quad100_height", ""),
                        photos.Current.GetAttribute("quad100_size", ""),
                        photos.Current.GetAttribute("quad100_download", "")
                    ),

                    Small = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("small_width", ""),
                        photos.Current.GetAttribute("small_height", ""),
                        photos.Current.GetAttribute("small_size", ""),
                        photos.Current.GetAttribute("small_download", "")
                    ),
                    Medium = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("medium_width", ""),
                        photos.Current.GetAttribute("medium_height", ""),
                        photos.Current.GetAttribute("medium_size", ""),
                        photos.Current.GetAttribute("medium_download", "")
                    ),
                    Portrait = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("portrait_width", ""),
                        photos.Current.GetAttribute("portrait_height", ""),
                        photos.Current.GetAttribute("portrait_size", ""),
                        photos.Current.GetAttribute("portrait_download", "")
                    ),
                    Standard = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("standard_width", ""),
                        photos.Current.GetAttribute("standard_height", ""),
                        photos.Current.GetAttribute("standard_size", ""),
                        photos.Current.GetAttribute("standard_download", "")
                    ),
                    Large = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("large_width", ""),
                        photos.Current.GetAttribute("large_height", ""),
                        photos.Current.GetAttribute("large_size", ""),
                        photos.Current.GetAttribute("large_download", "")
                    ),

                    VideoSmall = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("video_small_width", ""),
                        photos.Current.GetAttribute("video_small_height", ""),
                        photos.Current.GetAttribute("video_small_size", ""),
                        photos.Current.GetAttribute("video_small_download", "")
                    ),
                    VideoMedium = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("video_medium_width", ""),
                        photos.Current.GetAttribute("video_medium_height", ""),
                        photos.Current.GetAttribute("video_medium_size", ""),
                        photos.Current.GetAttribute("video_medium_download", "")
                    ),
                    VideoHD = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("video_hd_width", ""),
                        photos.Current.GetAttribute("video_hd_height", ""),
                        photos.Current.GetAttribute("video_hd_size", ""),
                        photos.Current.GetAttribute("video_hd_download", "")
                    ),
                    VideoMobileH263AMR = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("video_mobile_h263_amr_width", ""),
                        photos.Current.GetAttribute("video_mobile_h263_amr_height", ""),
                        photos.Current.GetAttribute("video_mobile_h263_amr_size", ""),
                        photos.Current.GetAttribute("video_mobile_h263_amr_download", "")
                    ),
                    VideoMobileH263AAC = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("video_mobile_h263_aac_width", ""),
                        photos.Current.GetAttribute("video_mobile_h263_aac_height", ""),
                        photos.Current.GetAttribute("video_mobile_h263_aac_size", ""),
                        photos.Current.GetAttribute("video_mobile_h263_aac_download", "")
                    ),
                    VideoMobileMPEGE4AMR = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("video_mobile_mpeg4_amr_width", ""),
                        photos.Current.GetAttribute("video_mobile_mpeg4_amr_height", ""),
                        photos.Current.GetAttribute("video_mobile_mpeg4_amr_size", ""),
                        photos.Current.GetAttribute("video_mobile_mpeg4_amr_download", "")
                    ),

                    Audio = new Domain.PhotoBlock(
                        photos.Current.GetAttribute("audio_width", ""),
                        photos.Current.GetAttribute("audio_height", ""),
                        photos.Current.GetAttribute("audio_size", ""),
                        photos.Current.GetAttribute("audio_download", "")
                    ),

                    Content = Helpers.GetNodeChildValue(photos.Current, "content"),
                    ContentText = Helpers.GetNodeChildValue(photos.Current, "content_text"),

                    BeforeDownloadType = Helpers.GetNodeChildValue(photos.Current, "before_download_type"),
                    BeforeDownloadURL = Helpers.GetNodeChildValue(photos.Current, "before_download_url"),
                    AfterDownloadType = Helpers.GetNodeChildValue(photos.Current, "after_download_type"),
                    AfterDownloadURL = Helpers.GetNodeChildValue(photos.Current, "after_download_url"),

                    AfterText = Helpers.GetNodeChildValue(photos.Current, "after_text"),

                    Tags = new List<string>(Helpers.GetNodeChildValue(photos.Current, "tags").Split(','))
                };

                result.Add(photoModel);
            }
            
            return result;
        }

        // * Upload photo
        // Implements http://www.23developer.com/api/photo-upload
        /// <summary>Upload a photo</summary>
        public int? Upload(string filename, string fileContentType, System.IO.Stream filestream) { return Upload(filename, fileContentType, filestream, null, null, null, null, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId) { return Upload(filename, fileContentType, filestream, userId, null, null, null, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId) { return Upload(filename, fileContentType, filestream, userId, albumId, null, null, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId, string title) { return Upload(filename, fileContentType, filestream, userId, albumId, title, null, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId, string title, string description) { return Upload(filename, fileContentType, filestream, userId, albumId, title, description, null, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId, string title, string description, string tags) { return Upload(filename, fileContentType, filestream, userId, albumId, title, description, tags, null); }
        /// <summary>Upload a photo</summary>
        public int? Upload(string filename, string fileContentType, System.IO.Stream filestream, int? userId, int? albumId, string title, string description, string tags, bool? publish)
        {
            // Verify required parameters
            if (filestream == null) return null;

            // Ensure that only relative filenames are sent
            int relativeFilenameSplit = filename.LastIndexOf('\\');
            string relativeFilename = (relativeFilenameSplit == -1 ? filename : filename.Substring(relativeFilenameSplit + 1));

            // Build request URL
            List<MultipartPostPart> data = new List<MultipartPostPart>
            {
                MultipartPostPart.CreateFormFilePart("file", relativeFilename, fileContentType, filestream)
            };

            if (userId != null) data.Add(MultipartPostPart.CreateFormPart("user_id", userId.ToString()));
            if (albumId != null) data.Add(MultipartPostPart.CreateFormPart("album_id", albumId.ToString()));
            if (title != null) data.Add(MultipartPostPart.CreateFormPart("title", title));
            if (description != null) data.Add(MultipartPostPart.CreateFormPart("description", description));
            if (tags != null) data.Add(MultipartPostPart.CreateFormPart("tags", tags));
            if (publish != null) data.Add(MultipartPostPart.CreateFormPart("publish", publish.Value ? "1" : "0"));

            // Do the request
            MessageReceivingEndpoint requestMessage = new MessageReceivingEndpoint(_provider.GetRequestUrl("/api/photo/upload", null), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            XPathNavigator responseMessage = _provider.DoRequest(requestMessage, data);
            if (responseMessage == null) return null;

            // Get the Photo id
            XPathNodeIterator photos = responseMessage.Select("/response/photo_id");
            if ((photos.MoveNext()) && (photos.Current != null)) return Helpers.ConvertStringToInteger(photos.Current.Value);
            
            // If nothing pops up, we'll return null
            return null;
        }

        // * Delete photo
        // Implements http://www.23developer.com/api/photo-delete
        /// <summary>Delete a photo given an id</summary>
        public bool Delete(int photoId)
        {
            // Build request URL
            List<string> requestUrlParameters = new List<string>();

            requestUrlParameters.Add("photo_id=" + photoId);

            // Do the request
            MessageReceivingEndpoint requestMessage = new MessageReceivingEndpoint(_provider.GetRequestUrl("/api/photo/delete", requestUrlParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator responseMessage = _provider.DoRequest(requestMessage);
            if (responseMessage == null) return false;

            // If nothing pops up, we'll return true
            return true;
        }

        // * Replace photo
        // Implements http://www.23developer.com/api/photo-replace
        /// <summary>
        /// Replace a photo thumbnail given an id
        /// </summary>
        /// <param name="photoId">Id of photo</param>
        /// <param name="filename">The original filename</param>
        /// <param name="fileContentType">The meta content type of the file</param>
        /// <param name="filestream">An input stream for reading the file</param>
        /// <returns></returns>
        public bool Replace(int photoId, string filename, string fileContentType, System.IO.Stream filestream)
        {
            // Build request URL
            List<string> requestUrlParameters = new List<string>();

            // Ensure that only relative filenames are sent
            int relativeFilenameSplit = filename.LastIndexOf('\\');
            string relativeFilename = (relativeFilenameSplit == -1 ? filename : filename.Substring(relativeFilenameSplit + 1));

            // Do the request
            MessageReceivingEndpoint requestMessage = new MessageReceivingEndpoint(_provider.GetRequestUrl("/api/photo/replace", requestUrlParameters), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            List<MultipartPostPart> data = new List<MultipartPostPart>
            {
                MultipartPostPart.CreateFormPart("photo_id", photoId.ToString()),
                MultipartPostPart.CreateFormFilePart("file", relativeFilename, fileContentType, filestream)
            };

            XPathNavigator responseMessage = _provider.DoRequest(requestMessage, data);
            if (responseMessage == null) return false;

            // If nothing pops up, we'll return true
            return true;
        }

        // * Update photo
        // Implements
        /// <summary>Update a photo given the id</summary>
        public bool Update(int photoId, int? albumId) { return Update(photoId, albumId, null, null, null, null); }
        /// <summary>Update a photo given the id</summary>
        public bool Update(int photoId, int? albumId, string title) { return Update(photoId, albumId, title, null, null, null); }
        /// <summary>Update a photo given the id</summary>
        public bool Update(int photoId, int? albumId, string title, string description) { return Update(photoId, albumId, title, description, null, null); }
        /// <summary>Update a photo given the id</summary>
        public bool Update(int photoId, int? albumId, string title, string description, string tags) { return Update(photoId, albumId, title, description, tags, null); }
        /// <summary>Update a photo given the id</summary>
        public bool Update(int photoId, int? albumId, string title, string description, string tags, bool? published)
        {
            // Build request URL
            List<MultipartPostPart> data = new List<MultipartPostPart>
            {
                MultipartPostPart.CreateFormPart("photo_id", photoId.ToString())
            };

            if (albumId != null) data.Add(MultipartPostPart.CreateFormPart("album_id", albumId.ToString()));
            if (title != null) data.Add(MultipartPostPart.CreateFormPart("title", title));
            if (description != null) data.Add(MultipartPostPart.CreateFormPart("description", description));
            if (tags != null) data.Add(MultipartPostPart.CreateFormPart("tags", tags));
            if (published != null) data.Add(MultipartPostPart.CreateFormPart("publish", published.Value ? "1" : "0"));

            // Do the request
            MessageReceivingEndpoint requestMessage = new MessageReceivingEndpoint(_provider.GetRequestUrl("/api/photo/update", null), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            XPathNavigator responseMessage = _provider.DoRequest(requestMessage, data);
            if (responseMessage == null) return false;

            // If nothing pops up, we'll return null
            return true;
        }

        // * Get upload token
        // Implements http://www.23developer.com/api/photo-get-upload-token
        public Domain.PhotoUploadToken GetUploadToken(string returnUrl, bool? backgroundReturn, int? userId, int? albumId, string title, string description, string tags, bool? publish, int? validMinutes, int? maxUploads)
        {
            if (String.IsNullOrEmpty(returnUrl)) return null;

            // Build request URL
            List<MultipartPostPart> data = new List<MultipartPostPart>
            {
                MultipartPostPart.CreateFormPart("return_url", returnUrl)
            };

            if (backgroundReturn != null) data.Add(MultipartPostPart.CreateFormPart("background_return_p", backgroundReturn.Value ? "1" : "0"));
            
            if (userId != null) data.Add(MultipartPostPart.CreateFormPart("user_id", userId.ToString()));
            if (albumId != null) data.Add(MultipartPostPart.CreateFormPart("album_id", albumId.ToString()));
            if (title != null) data.Add(MultipartPostPart.CreateFormPart("title", title));
            if (description != null) data.Add(MultipartPostPart.CreateFormPart("description", description));
            if (tags != null) data.Add(MultipartPostPart.CreateFormPart("tags", tags));

            if (publish != null) data.Add(MultipartPostPart.CreateFormPart("publish", publish.Value ? "1" : "0"));

            if (validMinutes != null) data.Add(MultipartPostPart.CreateFormPart("valid_minutes", validMinutes.ToString()));
            if (maxUploads != null) data.Add(MultipartPostPart.CreateFormPart("max_uploads", maxUploads.ToString()));

            // Do the request
            MessageReceivingEndpoint requestMessage = new MessageReceivingEndpoint(_provider.GetRequestUrl("/api/photo/get-upload-token", null), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            XPathNavigator responseMessage = _provider.DoRequest(requestMessage, data);
            if (responseMessage == null) return null;

            // Get the token
            XPathNodeIterator tokenNode = responseMessage.Select("/response");
            if (!tokenNode.MoveNext()) return null;

            Domain.PhotoUploadToken uploadToken = new Domain.PhotoUploadToken
            {
                UploadToken = Helpers.GetNodeChildValue(tokenNode.Current, "upload_token"),

                Title = Helpers.GetNodeChildValue(tokenNode.Current, "title"),
                Description = Helpers.GetNodeChildValue(tokenNode.Current, "description"),
                Tags = Helpers.GetNodeChildValue(tokenNode.Current, "tags"),

                Publish = (Helpers.GetNodeChildValue(tokenNode.Current, "publish") == "1"),

                UserId = Helpers.ConvertStringToInteger(Helpers.GetNodeChildValue(tokenNode.Current, "user_id")),
                AlbumId = Helpers.ConvertStringToInteger(Helpers.GetNodeChildValue(tokenNode.Current, "album_id")),

                ValidMinutes = Helpers.ConvertStringToInteger(Helpers.GetNodeChildValue(tokenNode.Current, "valid_minutes")),
                ValidUntil = Helpers.ConvertStringToInteger(Helpers.GetNodeChildValue(tokenNode.Current, "valid_until")),

                ReturnURL = Helpers.GetNodeChildValue(tokenNode.Current, "return_url")
            };

            // If nothing pops up, we'll return null
            return uploadToken;
        }

        // * Redeem upload token
        // Implements http://www.23developer.com/api/photo-redeem-upload-token
        public bool RedeemUploadToken(string filename, string fileContentType, System.IO.Stream filestream, string uploadToken)
        {
            // Verify required parameters
            if (filestream == null) return false;

            // Ensure that only relative filenames are sent
            int relativeFilenameSplit = filename.LastIndexOf('\\');
            string relativeFilename = (relativeFilenameSplit == -1 ? filename : filename.Substring(relativeFilenameSplit + 1));

            // Build request URL
            List<MultipartPostPart> data = new List<MultipartPostPart>
            {
                MultipartPostPart.CreateFormFilePart("file", relativeFilename, fileContentType, filestream),
                MultipartPostPart.CreateFormPart("upload_token", uploadToken)
            };

            // Do the request
            MessageReceivingEndpoint requestMessage = new MessageReceivingEndpoint(_provider.GetRequestUrl("/api/photo/redeem-upload-token", null), HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.AuthorizationHeaderRequest);

            XPathNavigator responseMessage = _provider.DoRequest(requestMessage, data);
            if (responseMessage == null) return false;
            
            // If nothing pops up, we'll return null
            return true;
        }
    }
}