using System.Data;
using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Data;

public static class UserAdminRepository
{
    public static DataTable GetAllUsers()
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT UserID, Username, Email, FullName, UserType, IsActive, CreatedAt
            FROM dbo.Users ORDER BY UserID
            """, cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static void UpdateUser(int adminUserId, int userId, string email, string fullName, bool isActive)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            UPDATE dbo.Users SET Email=@e, FullName=@f, IsActive=@a WHERE UserID=@id
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@e", email);
        cmd.Parameters.AddWithValue("@f", fullName);
        cmd.Parameters.AddWithValue("@a", isActive);
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
        ActivityLogRepository.Log(cn, adminUserId, "AdminUpdateUser", "User", userId, null);
    }

}
