using System.Collections.Generic;

namespace Visual
{
    public interface IUserService
    {
        Domain.User Get(int userId);
        List<Domain.User> GetList();
        List<Domain.User> GetList(UserListParameters requestParameters);
        int? Create(string email);
        int? Create(string email, string username);
        int? Create(string email, string username, string password);
        int? Create(string email, string username, string password, string fullName);
        int? Create(string email, string username, string password, string fullName, Timezone timezone);
        int? Create(string email, string username, string password, string fullName, Timezone timezone, bool siteAdmin);
		Domain.Session GetLoginToken(string userId);
        Domain.Session GetLoginToken(string userId, string returnUrl);
    }
}