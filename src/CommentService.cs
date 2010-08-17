using System.Collections.Generic;
using System.Web;
using System.Xml.XPath;
using DotNetOpenAuth.Messaging;
using System;

namespace Twentythree
{
    public interface ICommentService
    {
        List<Domain.Comment> GetList();
        List<Domain.Comment> GetList(CommentListParameters RequestParameters);
    }

    public class CommentService : ICommentService
    {
        private IAPIProvider Provider;

        public CommentService(IAPIProvider Provider)
        {
            this.Provider = Provider;
        }

        /// <summary>
        /// Returns a list of comments by default parameters
        /// </summary>
        public List<Domain.Comment> GetList()
        {
            return GetList(new CommentListParameters());
        }

        /// <summary>
        /// Returns a list of comments by specific parameters
        /// </summary>
        public List<Domain.Comment> GetList(CommentListParameters RequestParameters)
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            if (RequestParameters.ObjectId != null) RequestURLParameters.Add("object_id=" + RequestParameters.ObjectId.ToString());
            if (RequestParameters.ObjectType != CommentObjectType.Empty) RequestURLParameters.Add("order=" + RequestValues.Get(RequestParameters.ObjectType));

            if (RequestParameters.CommentId != null) RequestURLParameters.Add("comment_id=" + RequestParameters.CommentId.ToString());
            if (RequestParameters.CommentUserId != null) RequestURLParameters.Add("comment_user_id=" + RequestParameters.CommentUserId.ToString());

            if (RequestParameters.Search != null) RequestURLParameters.Add("search=" + HttpUtility.UrlEncode(RequestParameters.Search));

            if (RequestParameters.Order != GenericSort.Descending) RequestURLParameters.Add("order=" + RequestValues.Get(RequestParameters.Order));

            if (RequestParameters.PageOffset != null) RequestURLParameters.Add("p=" + RequestParameters.PageOffset.ToString());
            if (RequestParameters.Size != null) RequestURLParameters.Add("size=" + RequestParameters.Size.ToString());

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/comment/list", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return null;

            // List all the videos
            XPathNodeIterator Comments = ResponseMessage.Select("/response/comment");
            List<Domain.Comment> Result = new List<Domain.Comment>();

            while (Comments.MoveNext())
            {
                // Create the domain album
                Domain.Comment CommentModel = new Domain.Comment();

                CommentModel.CommentId = Helpers.ConvertStringToInteger(Comments.Current.GetAttribute("comment_id", ""));

                CommentModel.ObjectId = Helpers.ConvertStringToInteger(Comments.Current.GetAttribute("object_id", ""));
                switch (Comments.Current.GetAttribute("object_type", ""))
                {
                    case "album":
                        CommentModel.ObjectType = CommentObjectType.Album;
                        break;

                    case "photo":
                        CommentModel.ObjectType = CommentObjectType.Photo;
                        break;

                    default:
                        CommentModel.ObjectType = CommentObjectType.Empty;
                        break;
                }

                CommentModel.PrettyDate = Comments.Current.GetAttribute("pretty_date", "");
                CommentModel.PrettyTime = Comments.Current.GetAttribute("pretty_time", "");

                CommentModel.ShortDate = Comments.Current.GetAttribute("short_date", "");
                CommentModel.CreationDateANSI = Comments.Current.GetAttribute("creation_date_ansi", "");

                CommentModel.UserId = (!String.IsNullOrEmpty(Comments.Current.GetAttribute("user_id", "")) ? (int?)Helpers.ConvertStringToInteger(Comments.Current.GetAttribute("user_id", "")) : null);
                CommentModel.Name = Comments.Current.GetAttribute("name", "");
                CommentModel.Email = Comments.Current.GetAttribute("email", "");
                CommentModel.TruncatedName = Comments.Current.GetAttribute("truncated_name", "");

                CommentModel.Content = Helpers.GetNodeChildValue(Comments.Current, "content");
                CommentModel.ContentText = Helpers.GetNodeChildValue(Comments.Current, "content_text");

                Result.Add(CommentModel);
            }

            return Result;
        }
    }
}
