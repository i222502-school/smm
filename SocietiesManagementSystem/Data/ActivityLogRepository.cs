using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Data;

public static class ActivityLogRepository
{
    public static void Log(SqlConnection cn, int? userId, string actionType, string? entityType, int? entityId, string? details)
    {
        const string sql = """
            INSERT INTO dbo.ActivityLog (UserID, ActionType, EntityType, EntityId, Details)
            VALUES (@uid, @act, @ent, @eid, @det)
            """;

        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@uid", userId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@act", actionType);
        cmd.Parameters.AddWithValue("@ent", entityType ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@eid", entityId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@det", details ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }
}
