using SocietiesManagementSystem.Data;
using SocietiesManagementSystem.Models;
using SocietiesManagementSystem.Security;
using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Services;

public sealed class AuthService
{
    public SessionUser? Login(string username, string password)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            SELECT UserID, Username, FullName, Email, UserType, PasswordHash, Salt, IsActive
            FROM dbo.Users WHERE Username = @u
            """;

        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@u", username);

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        var salt = r.GetGuid(r.GetOrdinal("Salt"));
        var hash = (byte[])r["PasswordHash"];
        if (!PasswordHasher.Verify(password, salt, hash)) return null;
        if (!r.GetBoolean(r.GetOrdinal("IsActive"))) return null;

        return new SessionUser
        {
            UserId = r.GetInt32(r.GetOrdinal("UserID")),
            Username = r.GetString(r.GetOrdinal("Username")),
            FullName = r.GetString(r.GetOrdinal("FullName")),
            Email = r.GetString(r.GetOrdinal("Email")),
            UserType = r.GetString(r.GetOrdinal("UserType"))
        };
    }

    public (bool ok, string message) RegisterStudent(string username, string password, string email, string fullName)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length > 50)
            return (false, "Invalid username.");
        if (password.Length < 6)
            return (false, "Password must be at least 6 characters.");
        var salt = Guid.NewGuid();
        var hash = PasswordHasher.HashPassword(password, salt);

        try
        {
            using var cn = SqlConnectionFactory.CreateOpenConnection();
            const string sql = """
                INSERT INTO dbo.Users (Username, PasswordHash, Salt, Email, FullName, UserType)
                VALUES (@un, @ph, @salt, @em, @fn, N'Student')
                """;
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@un", username.Trim());
            cmd.Parameters.AddWithValue("@ph", hash);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@em", email.Trim());
            cmd.Parameters.AddWithValue("@fn", fullName.Trim());
            cmd.ExecuteNonQuery();
            ActivityLogRepository.Log(cn, null, "StudentRegistered", "User", null, username);
            return (true, "Account created. You can log in.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return (false, "Username already exists.");
        }
    }
}
