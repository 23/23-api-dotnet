using System.Collections.Generic;
using System.Xml.XPath;
using DotNetOpenAuth.Messaging;

namespace Visual
{
    public interface IApiProvider
    {
        XPathNavigator DoRequest(MessageReceivingEndpoint message);
        XPathNavigator DoRequest(MessageReceivingEndpoint message, List<MultipartPostPart> parameters);
        string GetRequestUrl(string method, List<string> parameters);
        void SetProxy(string uri, string username = null, string password = null, string domain = null);
    }
}