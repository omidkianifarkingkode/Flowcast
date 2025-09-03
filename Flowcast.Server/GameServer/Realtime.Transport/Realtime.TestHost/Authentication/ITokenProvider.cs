namespace Realtime.TestHost.Authentication;

public interface ITokenProvider
{
    string Create(string userId, string userEmail);
}
