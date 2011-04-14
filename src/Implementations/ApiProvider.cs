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

namespace Visual
{
    public class ApiProvider : IApiProvider
    {
        // * Consumer objects
        private WebConsumer _oAuthConsumer;
        private ServiceProviderDescription _oAuthProviderDescription = new ServiceProviderDescription();
        private InMemoryTokenManager _oAuthTokenManager;
        private WebProxy _proxy;

        // * Variables
        private string _consumerDomain;
        private string _consumerKey;
        private string _consumerSecret;

        private string _accessToken;
        private string _accessTokenSecret;

        // * Constructor
        /// <summary>
        /// Creates a 23 API service repository, that requires further authentication approval.
        /// When using this constructor, you should consider rewriting the token manager (InMemoryTokenManager) to let your application handle access tokens
        /// </summary>
        /// <param name="consumerDomain">Domain name</param>
        /// <param name="consumerKey">Consumer key</param>
        /// <param name="consumerSecret">Consumer secret</param>
        public ApiProvider(string consumerDomain, string consumerKey, string consumerSecret)
            : this(consumerDomain, consumerKey, consumerSecret, null, null)
        {

        }

        /// <summary>
        /// Creates a 23 API service repository, that doesn't require further authentication. Account must be "privileged"
        /// </summary>
        /// <param name="consumerDomain">Domain name</param>
        /// <param name="consumerKey">Consumer key</param>
        /// <param name="consumerSecret">Consumer secret</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="accessTokenSecret">Access token secret</param>
        public ApiProvider(string consumerDomain, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            // Save the authentication keys
            _consumerDomain = consumerDomain;

            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;

            // Open the OAuth consumer connection
            _oAuthProviderDescription.AccessTokenEndpoint = new MessageReceivingEndpoint("http://api.visualplatform.net/oauth/access_token", HttpDeliveryMethods.GetRequest);
            _oAuthProviderDescription.RequestTokenEndpoint = new MessageReceivingEndpoint("http://api.visualplatform.net/oauth/request_token", HttpDeliveryMethods.GetRequest);
            _oAuthProviderDescription.ProtocolVersion = ProtocolVersion.V10a;
            _oAuthProviderDescription.UserAuthorizationEndpoint = new MessageReceivingEndpoint("http://api.visualplatform.net/oauth/authorize", HttpDeliveryMethods.GetRequest);
            _oAuthProviderDescription.TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() };

            _oAuthTokenManager = new InMemoryTokenManager(_consumerKey, _consumerSecret);

            _oAuthConsumer = new WebConsumer(_oAuthProviderDescription, _oAuthTokenManager);

            if (accessToken != null)
            {
                _accessToken = accessToken;
                _accessTokenSecret = accessTokenSecret;

                _oAuthTokenManager.ExpireRequestTokenAndStoreNewAccessToken(_consumerKey, "", _accessToken, _accessTokenSecret);
            }

            _oAuthConsumer.Channel.AssertBoundary();
        }

        // ***** Internal functions *****
        public XPathNavigator DoRequest(MessageReceivingEndpoint message)
        {
            return DoRequest(message, null);
        }

        public XPathNavigator DoRequest(MessageReceivingEndpoint message, List<MultipartPostPart> parameters)
        {
            // Verify authentication
            if (_accessToken == null)
            {
                AuthorizedTokenResponse accessTokenResponse = _oAuthConsumer.ProcessUserAuthorization();
                if (accessTokenResponse != null) _accessToken = accessTokenResponse.AccessToken;
                else if (_accessToken == null) _oAuthConsumer.Channel.Send(_oAuthConsumer.PrepareRequestUserAuthorization());
            }

            HttpWebRequest request = (parameters == null ? _oAuthConsumer.PrepareAuthorizedRequest(message, _accessToken) : _oAuthConsumer.PrepareAuthorizedRequest(message, _accessToken, parameters));
            if (_proxy != null)
                request.Proxy = _proxy;
            IncomingWebResponse response = _oAuthConsumer.Channel.WebRequestHandler.GetResponse(request);

            XDocument responseDocument = XDocument.Load(XmlReader.Create(response.GetResponseReader()));

            // Establish navigator and validate response
            XPathNavigator responseNavigation = responseDocument.CreateNavigator();

            XPathNodeIterator responseCheckIterator = responseNavigation.Select("/response");
            if (responseCheckIterator.Count == 0) return null;
            while (responseCheckIterator.MoveNext())
            {
                if (responseCheckIterator.Current == null) return null;

                if (responseCheckIterator.Current.GetAttribute("status", "") != "ok")
                {
                    return null;
                }
            }

            // All should be good, return the document
            return responseNavigation;
        }

        public string GetRequestUrl(string method, List<string> parameters)
        {
            return "http://" + _consumerDomain + method + (parameters != null ? (parameters.Count > 0 ? "?" + String.Join("&", parameters.ToArray()) : "") : "");
        }

        public void SetProxy(string uri, string username = null, string password = null, string domain = null)
        {
            _proxy = new WebProxy(new Uri(uri));
            if ((!string.IsNullOrEmpty(username)) || (!string.IsNullOrEmpty(password)))
                _proxy.Credentials = !string.IsNullOrEmpty(domain) ? new NetworkCredential(username, password, domain) : new NetworkCredential(username, password);
        }
    }
}