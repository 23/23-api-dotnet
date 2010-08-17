using System;
using System.Collections.Generic;
using System.Web;
using System.Xml.XPath;
using DotNetOpenAuth.Messaging;

namespace Twentythree
{
    public interface IUserService
    {
        List<Domain.User> GetList();
        List<Domain.User> GetList(UserListParameters RequestParameters);
        int? Create(string Email);
        int? Create(string Email, string Username);
        int? Create(string Email, string Username, string Password);
        int? Create(string Email, string Username, string Password, string FullName);
        int? Create(string Email, string Username, string Password, string FullName, Timezone Timezone);
        int? Create(string Email, string Username, string Password, string FullName, Timezone Timezone, bool SiteAdmin);
    }

    public class UserService : IUserService
    {
        private IAPIProvider Provider;

        public UserService(IAPIProvider Provider)
        {
            this.Provider = Provider;
        }

        // * Get User list
        // Implements http://www.23developer.com/api/user-list
        /// <summary>
        /// Returns a list of users by default parameters
        /// </summary>
        public List<Domain.User> GetList()
        {
            return this.GetList(new UserListParameters());
        }

        /// <summary>
        /// Returns a list of users by specific parameters
        /// </summary>
        public List<Domain.User> GetList(UserListParameters RequestParameters)
        {
            // Build request URL
            List<string> RequestURLParameters = new List<string>();

            if (RequestParameters.UserId != null) RequestURLParameters.Add("user_id=" + RequestParameters.UserId.ToString());

            if (RequestParameters.Search != null) RequestURLParameters.Add("search=" + HttpUtility.UrlEncode(RequestParameters.Search));

            if (RequestParameters.OrderBy != UserListSort.DisplayName) RequestURLParameters.Add("orderby=" + RequestValues.Get(RequestParameters.OrderBy));
            if (RequestParameters.Order != GenericSort.Descending) RequestURLParameters.Add("order=" + RequestValues.Get(RequestParameters.Order));

            if (RequestParameters.PageOffset != null) RequestURLParameters.Add("p=" + RequestParameters.PageOffset.ToString());
            if (RequestParameters.Size != null) RequestURLParameters.Add("size=" + RequestParameters.Size.ToString());

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/user/list", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return null;

            // List all the videos
            XPathNodeIterator Users = ResponseMessage.Select("/response/user");
            List<Domain.User> Result = new List<Domain.User>();

            while (Users.MoveNext())
            {
                // Create the domain User
                Domain.User UserModel = new Domain.User();

                UserModel.UserId = Helpers.ConvertStringToInteger(Users.Current.GetAttribute("user_id", ""));

                UserModel.Username = Users.Current.GetAttribute("username", "");
                UserModel.DisplayName = Users.Current.GetAttribute("display_name", "");
                UserModel.URL = Users.Current.GetAttribute("url", "");
                UserModel.FullName = Users.Current.GetAttribute("full_name", "");
                UserModel.Email = Users.Current.GetAttribute("email", "");
                UserModel.SiteAdmin = (Users.Current.GetAttribute("site_admin", "") != "f");

                UserModel.AboutAbstract = Helpers.GetNodeChildValue(Users.Current, "about_abstract");

                Result.Add(UserModel);
            }
            
            return Result;
        }

        // * Create user
        // Implements http://www.23developer.com/api/user-create
        /// <summary>Create a user specified by an e-mail address</summary>
        public int? Create(string Email) { return Create(Email, null, null, null, Timezone.CET, false); }
        /// <summary>Create a user specified by an e-mail address and username</summary>
        public int? Create(string Email, string Username) { return Create(Email, Username, null, null, Timezone.CET, false); }
        /// <summary>Create a user specified by an e-mail address, username and password</summary>
        public int? Create(string Email, string Username, string Password) { return Create(Email, Username, Password, null, Timezone.CET, false); }
        /// <summary>Create a user specified by an e-mail address, username, password and full name</summary>
        public int? Create(string Email, string Username, string Password, string FullName) { return Create(Email, Username, Password, FullName, Timezone.CET, false); }
        /// <summary>Create a user specified by an e-mail address, username, password, full name and timezone</summary>
        public int? Create(string Email, string Username, string Password, string FullName, Timezone Timezone) { return Create(Email, Username, Password, FullName, Timezone, false); }
        /// <summary>Create a user specified by an e-mail address, username, password, full name, timezone and site admin rigts specification</summary>
        public int? Create(string Email, string Username, string Password, string FullName, Timezone Timezone, bool SiteAdmin)
        {
            // Verify required parameters
            if (String.IsNullOrEmpty(Email)) return null;

            // Build request URL
            List<string> RequestURLParameters = new List<string>();
            
            RequestURLParameters.Add("email=" + HttpUtility.UrlEncode(Email));
            if (!String.IsNullOrEmpty(Username)) RequestURLParameters.Add("username=" + HttpUtility.UrlEncode(Username));
            if (!String.IsNullOrEmpty(Password)) RequestURLParameters.Add("password=" + HttpUtility.UrlEncode(Password));
            if (!String.IsNullOrEmpty(FullName)) RequestURLParameters.Add("full_name=" + HttpUtility.UrlEncode(FullName));
            RequestURLParameters.Add("timezone=" + HttpUtility.UrlEncode(RequestValues.Get(Timezone)));
            if (SiteAdmin != false) RequestURLParameters.Add("site_admin=1");

            // Do the request
            MessageReceivingEndpoint RequestMessage = new MessageReceivingEndpoint(this.Provider.GetRequestURL("/api/user/create", RequestURLParameters), HttpDeliveryMethods.GetRequest);

            XPathNavigator ResponseMessage = this.Provider.DoRequest(RequestMessage);
            if (ResponseMessage == null) return null;

            // Get the User id
            XPathNodeIterator Users = ResponseMessage.Select("/response/user_id");
            if (Users.MoveNext()) return Helpers.ConvertStringToInteger(Users.Current.Value);
            
            // If nothing pops up, we'll return null
            return null;
        }
    }
}