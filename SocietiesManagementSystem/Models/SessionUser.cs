namespace SocietiesManagementSystem.Models;

public sealed class SessionUser
{
    public int UserId { get; init; }
    public string Username { get; init; } = "";
    public string FullName { get; init; } = "";
    public string Email { get; init; } = "";
    public string UserType { get; init; } = "";
}
