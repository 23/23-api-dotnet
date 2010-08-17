using System.Collections.Generic;
using System.Xml.XPath;
using DotNetOpenAuth.Messaging;
using System.Web;
using System;

namespace Twentythree
{
    public interface ITagService
    {
        List<Domain.Tag> GetList();
        List<Domain.Tag> GetList(TagListParameters RequestParameters);
        List<Domain.Tag> GetRelatedList(string Name);
    }

    public class TagService : ITagService
    {
        private IAPIProvider Provider;

        public TagService(IAPIProvider Provider)
        {
            this.Provider = Provider;
        }

        /// <summary>
        /// Get a list of tags defined by the default request parameters
        /// </summary>
        /// <returns></returns>
        public List<Domain.Tag> GetList()
        {
            return GetList(new TagListParameters());
        }

        /// <summary>
        /// Get a list of tags defined by the request parameters
        /// </summary>
        /// <param name="RequestParameters"></param>
        /// <returns></returns>
        public List<Domain.Tag> GetList(TagListParameters RequestParameters)
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            if (RequestParameters.Search != null) RequestURLParameters.Add("search=" + HttpUtility.UrlEncode(RequestParameters.Search));

            if (RequestParameters.ReformatTags != false) RequestURLParameters.Add("reformat_tags_p=1");
            if (RequestParameters.ExcludeMachineTags != false) RequestURLParameters.Add("exclude_machine_tags_p=1");

            if (RequestParameters.OrderBy != TagListSort.Tag) RequestURLParameters.Add("orderby=" + RequestValues.Get(RequestParameters.OrderBy));
            if (RequestParameters.Order != GenericSort.Descending) RequestURLParameters.Add("order=" + RequestValues.Get(RequestParameters.Order));

            if (RequestParameters.PageOffset != null) RequestURLParameters.Add("p=" + RequestParameters.PageOffset.ToString());
            if (RequestParameters.Size != null) RequestURLParameters.Add("size=" + RequestParameters.Size.ToString());

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/tag/list", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return null;

            // List all the videos
            XPathNodeIterator Tags = ResponseMessage.Select("/response/tag");
            List<Domain.Tag> Result = new List<Domain.Tag>();

            while (Tags.MoveNext())
            {
                // Create the domain Tag
                Domain.Tag TagModel = new Domain.Tag();

                TagModel.Name = Tags.Current.GetAttribute("tag", "");
                TagModel.URL = Tags.Current.GetAttribute("url", "");
                TagModel.Count = Helpers.ConvertStringToInteger(Tags.Current.GetAttribute("count", ""));

                Result.Add(TagModel);
            }

            return Result;
        }

        public List<Domain.Tag> GetRelatedList(string Name)
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            RequestURLParameters.Add("tag=" + HttpUtility.UrlEncode(Name));

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/tag/related", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return null;

            // List all the videos
            XPathNodeIterator Tags = ResponseMessage.Select("/response/relatedtag");
            List<Domain.Tag> Result = new List<Domain.Tag>();

            while (Tags.MoveNext())
            {
                // Create the domain Tag
                Domain.Tag TagModel = new Domain.Tag();

                TagModel.Name = Tags.Current.GetAttribute("tag", "");
                TagModel.URL = Tags.Current.GetAttribute("url", "");
                TagModel.Count = null;

                Result.Add(TagModel);
            }

            return Result;
        }
    }
}
