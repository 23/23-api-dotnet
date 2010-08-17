using System.Collections.Generic;
using System.Xml.XPath;
using DotNetOpenAuth.Messaging;
using System.Web;
using System;

namespace Twentythree
{
    public interface ISiteService
    {
        Domain.Site GetSite();
    }

    public class SiteService : ISiteService
    {
        private IAPIProvider Provider;

        public SiteService(IAPIProvider Provider)
        {
            this.Provider = Provider;
        }

        /// <summary>
        /// Method for getting a Site object containing information about the site
        /// </summary>
        /// <returns></returns>
        public Domain.Site GetSite()
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/site/get", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return null;

            // Get the site id
            XPathNodeIterator SiteIdToken = ResponseMessage.Select("/response/site_id");
            if (!SiteIdToken.MoveNext()) return null;

            // Get the site name
            XPathNodeIterator SiteNameToken = ResponseMessage.Select("/response/site_name");
            if (!SiteNameToken.MoveNext()) return null;

            // Get the product key
            XPathNodeIterator ProductKeyToken = ResponseMessage.Select("/response/product_key");
            if (!ProductKeyToken.MoveNext()) return null;

            // Get the allow signup variable
            XPathNodeIterator AllowSignupToken = ResponseMessage.Select("/response/allow_signup_p");
            if (!AllowSignupToken.MoveNext()) return null;

            // Get the site key
            XPathNodeIterator SiteKeyToken = ResponseMessage.Select("/response/site_key");
            if (!SiteKeyToken.MoveNext()) return null;

            // Get the license id
            XPathNodeIterator LicenseIdToken = ResponseMessage.Select("/response/license_id");
            if (!LicenseIdToken.MoveNext()) return null;

            // Get the domain
            XPathNodeIterator DomainToken = ResponseMessage.Select("/response/domain");
            if (!DomainToken.MoveNext()) return null;

            // Create result
            try
            {
                Domain.Site Result = new Domain.Site()
                {
                    SiteId = Helpers.ConvertStringToInteger(SiteIdToken.Current.Value),
                    SiteName = SiteNameToken.Current.Value,
                    SiteKey = SiteKeyToken.Current.Value,
                    ProductKey = ProductKeyToken.Current.Value,
                    AllowSignup = (AllowSignupToken.Current.Value != "f"),
                    LicenseId = Helpers.ConvertStringToInteger(LicenseIdToken.Current.Value),
                    Domain = DomainToken.Current.Value
                };

                return Result;
            }
            catch
            {
                return null;
            }
        }
    }
}
