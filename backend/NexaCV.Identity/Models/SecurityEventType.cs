namespace NexaCV.Identity.Models;

public enum SecurityEventType
{
    LoginSuccess,
    LoginFailure,
    Logout,
    PasswordChanged,
    Registration,
    TokenRefresh,
    TokenRefreshSuspicious
}
