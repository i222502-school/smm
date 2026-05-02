using System.Data;
using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Data;

public static class TaskRepository
{
    public static DataTable GetTasksForSociety(int societyId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT t.TaskID, t.Title, t.Description, t.DueDate, t.Status,
                   t.AssignedToUserID,
                   assignee.FullName AS AssignedTo, assigner.FullName AS AssignedBy
            FROM dbo.Tasks t
            INNER JOIN dbo.Users assignee ON assignee.UserID = t.AssignedToUserID
            INNER JOIN dbo.Users assigner ON assigner.UserID = t.AssignedByUserID
            WHERE t.SocietyID=@sid ORDER BY t.CreatedAt DESC
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@sid", societyId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static DataTable GetMyTasks(int userId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT t.TaskID, t.Title, t.Description, t.DueDate, t.Status, s.Name AS SocietyName
            FROM dbo.Tasks t
            INNER JOIN dbo.Societies s ON s.SocietyID = t.SocietyID
            WHERE t.AssignedToUserID=@uid ORDER BY t.DueDate
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@uid", userId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static void AddTask(int societyId, int assignedByUserId, int assignedToUserId,
        string title, string description, DateTime? dueDate)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            INSERT INTO dbo.Tasks (SocietyID, Title, Description, AssignedByUserID, AssignedToUserID, DueDate, Status)
            VALUES (@sid, @t, @d, @by, @to, @due, N'Pending')
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@sid", societyId);
        cmd.Parameters.AddWithValue("@t", title.Trim());
        cmd.Parameters.AddWithValue("@d", description.Trim());
        cmd.Parameters.AddWithValue("@by", assignedByUserId);
        cmd.Parameters.AddWithValue("@to", assignedToUserId);
        cmd.Parameters.AddWithValue("@due", dueDate ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
        ActivityLogRepository.Log(cn, assignedByUserId, "TaskAssigned", "Society", societyId, title);
    }

    public static void UpdateTaskStatus(int taskId, int userId, string status)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            UPDATE dbo.Tasks SET Status=@st
            WHERE TaskID=@tid AND (AssignedToUserID=@uid OR AssignedByUserID=@uid)
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@st", status);
        cmd.Parameters.AddWithValue("@tid", taskId);
        cmd.Parameters.AddWithValue("@uid", userId);
        cmd.ExecuteNonQuery();
    }
}
