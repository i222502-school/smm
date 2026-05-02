using System.Data;
using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Data;

public static class ActivityLogQueryRepository
{
    public static DataTable GetRecent(int top = 500)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var cmd = new SqlCommand(
            """
            SELECT TOP (@top) l.LogID, l.LoggedAt, l.ActionType, l.EntityType, l.EntityId, l.Details,
                   u.Username
            FROM dbo.ActivityLog l
            LEFT JOIN dbo.Users u ON u.UserID = l.UserID
            ORDER BY l.LogID DESC
            """, cn);
        cmd.Parameters.AddWithValue("@top", top);
        using var da = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }
}
