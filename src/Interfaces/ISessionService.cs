namespace Visual
{
    public interface ISessionService
    {
        Domain.Session GetToken();
        Domain.Session GetToken(string returnUrl);
    }
}