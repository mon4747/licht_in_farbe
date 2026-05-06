using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;
    public static async Task<AuthState> DoAuth(int maxRetries = 5)
    {
        if(AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }

        AuthState = AuthState.Authenticating;

        int retries = 0;
        while (AuthState == AuthState.Authenticating && retries < maxRetries)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if(AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
            {
                AuthState = AuthState.Authenticated;
                break;
            }

            retries++;
            await Task.Delay(1000);
        }
        return AuthState;           
    }
}


 