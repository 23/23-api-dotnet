using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;
using System.Net;

namespace Twentythree
{
    public interface IAPIProvider
    {
        XPathNavigator DoRequest(MessageReceivingEndpoint AMessage);
        XPathNavigator DoRequest(MessageReceivingEndpoint AMessage, List<MultipartPostPart> AParameters);
        string GetRequestURL(string Method, List<string> Parameters);
    }

    public class APIProvider : IAPIProvider
    {
        // * Consumer objects
        private DotNetOpenAuth.OAuth.WebConsumer OAuthConsumer;
        private ServiceProviderDescription OAuthProviderDescription = new ServiceProviderDescription();
        private InMemoryTokenManager OAuthTokenManager;

        // * Variables
        private string ConsumerDomain;
        private string ConsumerKey;
        private string ConsumerSecret;

        private string AccessToken = null;
        private string AccessTokenSecret = null;

        // * Constructor
        /// <summary>
        /// Creates a 23 API service repository, that requires further authentication approval.
        /// When using this constructor, you should consider rewriting the token manager (InMemoryTokenManager) to let your application handle access tokens
        /// </summary>
        /// <param name="ConsumerDomain">Domain name</param>
        /// <param name="ConsumerKey">Consumer key</param>
        /// <param name="ConsumerSecret">Consumer secret</param>
        public APIProvider(string ConsumerDomain, string ConsumerKey, string ConsumerSecret)
            : this(ConsumerDomain, ConsumerKey, ConsumerSecret, null, null)
        {

        }

        /// <summary>
        /// Creates a 23 API service repository, that doesn't require further authentication. Account must be "privileged"
        /// </summary>
        /// <param name="ConsumerDomain">Domain name</param>
        /// <param name="ConsumerKey">Consumer key</param>
        /// <param name="ConsumerSecret">Consumer secret</param>
        /// <param name="AccessToken">Access token</param>
        /// <param name="AccessTokenSecret">Access token secret</param>
        public APIProvider(string ConsumerDomain, string ConsumerKey, string ConsumerSecret, string AccessToken, string AccessTokenSecret)
        {
            // Save the authentication keys
            this.ConsumerDomain = ConsumerDomain;

            this.ConsumerKey = ConsumerKey;
            this.ConsumerSecret = ConsumerSecret;

            // Open the OAuth consumer connection
            this.OAuthProviderDescription.AccessTokenEndpoint = new MessageReceivingEndpoint("http://api.visualplatform.net/oauth/access_token", HttpDeliveryMethods.GetRequest);
            this.OAuthProviderDescription.RequestTokenEndpoint = new MessageReceivingEndpoint("http://api.visualplatform.net/oauth/request_token", HttpDeliveryMethods.GetRequest);
            this.OAuthProviderDescription.ProtocolVersion = ProtocolVersion.V10a;
            this.OAuthProviderDescription.UserAuthorizationEndpoint = new MessageReceivingEndpoint("http://api.visualplatform.net/oauth/authorize", HttpDeliveryMethods.GetRequest);
            this.OAuthProviderDescription.TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() };

            this.OAuthTokenManager = new InMemoryTokenManager(this.ConsumerKey, this.ConsumerSecret);

            this.OAuthConsumer = new WebConsumer(this.OAuthProviderDescription, this.OAuthTokenManager);

            if (AccessToken != null)
            {
                this.AccessToken = AccessToken;
                this.AccessTokenSecret = AccessTokenSecret;

                this.OAuthTokenManager.ExpireRequestTokenAndStoreNewAccessToken(this.ConsumerKey, "", this.AccessToken, this.AccessTokenSecret);
            }

            this.OAuthConsumer.Channel.AssertBoundary();
        }

        // ***** Internal functions *****
        public XPathNavigator DoRequest(MessageReceivingEndpoint AMessage)
        {
            return DoRequest(AMessage, null);
        }

        public XPathNavigator DoRequest(MessageReceivingEndpoint AMessage, List<MultipartPostPart> AParameters)
        {
            // Verify authentication
            if (this.AccessToken == null)
            {
                AuthorizedTokenResponse AccessTokenResponse = this.OAuthConsumer.ProcessUserAuthorization();
                if (AccessTokenResponse != null) this.AccessToken = AccessTokenResponse.AccessToken;
                else if (this.AccessToken == null) this.OAuthConsumer.Channel.Send(this.OAuthConsumer.PrepareRequestUserAuthorization());
            }

            HttpWebRequest Request = (AParameters == null ? this.OAuthConsumer.PrepareAuthorizedRequest(AMessage, this.AccessToken) : this.OAuthConsumer.PrepareAuthorizedRequest(AMessage, this.AccessToken, AParameters));
            IncomingWebResponse Response = this.OAuthConsumer.Channel.WebRequestHandler.GetResponse(Request);

            XDocument ResponseDocument = XDocument.Load(XmlReader.Create(Response.GetResponseReader()));

            // Establish navigator and validate response
            XPathNavigator ResponseNavigation = ResponseDocument.CreateNavigator();

            XPathNodeIterator ResponseCheckIterator = ResponseNavigation.Select("/response");
            if (ResponseCheckIterator.Count == 0) return null;
            while (ResponseCheckIterator.MoveNext())
            {
                if (ResponseCheckIterator.Current.GetAttribute("status", "") != "ok")
                {
                    return null;
                }
            }

            // All should be good, return the document
            return ResponseNavigation;
        }

        public string GetRequestURL(string Method, List<string> Parameters)
        {
            return "http://" + this.ConsumerDomain + Method + (Parameters != null ? (Parameters.Count > 0 ? "?" + String.Join("&", Parameters.ToArray()) : "") : "");
        }
    }
}
