using SocietiesManagementSystem.Models;

namespace SocietiesManagementSystem;

public static class AppSession
{
    public static SessionUser? CurrentUser { get; set; }

    public static void SignOut() => CurrentUser = null;
}
