using System.Data;
using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Data;

public static class AnnouncementRepository
{
    public static DataTable GetForStudentHome()
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT a.Title, a.Body, a.PostedAt,
                   ISNULL(s.Name, N'University') AS Scope,
                   u.FullName AS PostedBy
            FROM dbo.Announcements a
            INNER JOIN dbo.Users u ON u.UserID = a.PostedByUserID
            LEFT JOIN dbo.Societies s ON s.SocietyID = a.SocietyID
            WHERE a.SocietyID IS NULL OR s.Status = N'Approved'
            ORDER BY a.PostedAt DESC
            """, cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static DataTable GetForSociety(int societyId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT a.AnnouncementID, a.Title, a.Body, a.PostedAt, u.FullName AS PostedBy
            FROM dbo.Announcements a
            INNER JOIN dbo.Users u ON u.UserID = a.PostedByUserID
            WHERE a.SocietyID=@sid OR a.SocietyID IS NULL
            ORDER BY a.PostedAt DESC
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@sid", societyId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static void Post(int? societyId, int postedByUserId, string title, string body)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            INSERT INTO dbo.Announcements (SocietyID, PostedByUserID, Title, Body)
            VALUES (@sid, @uid, @t, @b)
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@sid", societyId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@uid", postedByUserId);
        cmd.Parameters.AddWithValue("@t", title.Trim());
        cmd.Parameters.AddWithValue("@b", body.Trim());
        cmd.ExecuteNonQuery();
        ActivityLogRepository.Log(cn, postedByUserId, "AnnouncementPosted", societyId.HasValue ? "Society" : "Global", societyId, title);
    }
}
