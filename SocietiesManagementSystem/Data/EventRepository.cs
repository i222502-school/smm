using System.Data;
using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Data;

public static class EventRepository
{
    public static DataTable GetUpcomingApprovedEvents()
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT e.EventID, e.Title, e.Venue, e.EventStart, e.EventEnd, e.MaxParticipants,
                   s.Name AS SocietyName,
                   (SELECT COUNT(*) FROM dbo.EventRegistrations r WHERE r.EventID=e.EventID) AS RegisteredCount
            FROM dbo.Events e
            INNER JOIN dbo.Societies s ON s.SocietyID = e.SocietyID
            WHERE e.Status = N'Approved' AND e.EventEnd >= SYSUTCDATETIME()
            ORDER BY e.EventStart
            """, cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static DataTable GetEventsForSociety(int societyId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT EventID, Title, Venue, EventStart, EventEnd, MaxParticipants, Status,
                   (SELECT COUNT(*) FROM dbo.EventRegistrations r WHERE r.EventID=e.EventID) AS RegisteredCount
            FROM dbo.Events e WHERE SocietyID=@sid ORDER BY EventStart DESC
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@sid", societyId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static DataTable GetPendingEventsForAdmin()
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT e.EventID, e.Title, e.Venue, e.EventStart, s.Name AS SocietyName
            FROM dbo.Events e
            INNER JOIN dbo.Societies s ON s.SocietyID = e.SocietyID
            WHERE e.Status = N'PendingAdminApproval'
            ORDER BY e.EventStart
            """, cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static int CreateEvent(int societyId, int creatorUserId, string title, string description,
        string venue, DateTime start, DateTime end, int? maxParticipants, bool submitForApproval)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        var status = submitForApproval ? "PendingAdminApproval" : "Draft";
        const string sql = """
            INSERT INTO dbo.Events (SocietyID, Title, Description, Venue, EventStart, EventEnd, MaxParticipants, Status, CreatedByUserID)
            OUTPUT INSERTED.EventID
            VALUES (@sid, @t, @d, @v, @s, @en, @max, @st, @uid)
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@sid", societyId);
        cmd.Parameters.AddWithValue("@t", title.Trim());
        cmd.Parameters.AddWithValue("@d", description.Trim());
        cmd.Parameters.AddWithValue("@v", venue.Trim());
        cmd.Parameters.AddWithValue("@s", start);
        cmd.Parameters.AddWithValue("@en", end);
        cmd.Parameters.AddWithValue("@max", maxParticipants ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@st", status);
        cmd.Parameters.AddWithValue("@uid", creatorUserId);
        var id = (int)cmd.ExecuteScalar()!;
        ActivityLogRepository.Log(cn, creatorUserId, "EventCreated", "Event", id, status);
        return id;
    }

    public static void UpdateEvent(int eventId, int societyId, string title, string description,
        string venue, DateTime start, DateTime end, int? maxParticipants, string status)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            UPDATE dbo.Events SET Title=@t, Description=@d, Venue=@v, EventStart=@s, EventEnd=@en,
                   MaxParticipants=@max, Status=@st
            WHERE EventID=@eid AND SocietyID=@sid
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@t", title.Trim());
        cmd.Parameters.AddWithValue("@d", description.Trim());
        cmd.Parameters.AddWithValue("@v", venue.Trim());
        cmd.Parameters.AddWithValue("@s", start);
        cmd.Parameters.AddWithValue("@en", end);
        cmd.Parameters.AddWithValue("@max", maxParticipants ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@st", status);
        cmd.Parameters.AddWithValue("@eid", eventId);
        cmd.Parameters.AddWithValue("@sid", societyId);
        cmd.ExecuteNonQuery();
    }

    public static void CancelEvent(int eventId, int societyId, int userId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            UPDATE dbo.Events SET Status=N'Cancelled' WHERE EventID=@eid AND SocietyID=@sid
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@eid", eventId);
        cmd.Parameters.AddWithValue("@sid", societyId);
        cmd.ExecuteNonQuery();
        ActivityLogRepository.Log(cn, userId, "EventCancelled", "Event", eventId, null);
    }

    public static void AdminApproveEvent(int eventId, int adminUserId, bool approve)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            UPDATE dbo.Events
            SET Status = CASE WHEN @ok=1 THEN N'Approved' ELSE N'Cancelled' END,
                ApprovedByUserID = CASE WHEN @ok=1 THEN @aid ELSE ApprovedByUserID END,
                ApprovedAt = CASE WHEN @ok=1 THEN SYSUTCDATETIME() ELSE ApprovedAt END
            WHERE EventID=@eid AND Status=N'PendingAdminApproval'
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@ok", approve);
        cmd.Parameters.AddWithValue("@aid", adminUserId);
        cmd.Parameters.AddWithValue("@eid", eventId);
        var n = cmd.ExecuteNonQuery();
        if (n > 0)
            ActivityLogRepository.Log(cn, adminUserId, approve ? "AdminEventApproved" : "AdminEventRejected", "Event", eventId, null);
    }

    public static (bool ok, string msg) RegisterStudentForEvent(int userId, int eventId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string q = """
            SELECT e.MaxParticipants, e.Status,
                   (SELECT COUNT(*) FROM dbo.EventRegistrations r WHERE r.EventID=e.EventID) AS Cnt
            FROM dbo.Events e WHERE e.EventID=@eid
            """;
        int? max = null;
        string? st = null;
        int cnt = 0;
        using (var cmd = new SqlCommand(q, cn))
        {
            cmd.Parameters.AddWithValue("@eid", eventId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return (false, "Event not found.");
            st = r.GetString(r.GetOrdinal("Status"));
            if (!r.IsDBNull(r.GetOrdinal("MaxParticipants")))
                max = r.GetInt32(r.GetOrdinal("MaxParticipants"));
            cnt = r.GetInt32(r.GetOrdinal("Cnt"));
        }

        if (st != "Approved") return (false, "Event is not open for registration.");
        if (max.HasValue && cnt >= max.Value) return (false, "Event is full.");

        try
        {
            const string ins = """
                INSERT INTO dbo.EventRegistrations (EventID, UserID) VALUES (@eid, @uid)
                """;
            using var cmd = new SqlCommand(ins, cn);
            cmd.Parameters.AddWithValue("@eid", eventId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.ExecuteNonQuery();
            ActivityLogRepository.Log(cn, userId, "EventRegistered", "Event", eventId, null);
            return (true, "Registered successfully.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return (false, "You are already registered.");
        }
    }

    public static DataTable GetTicketsForUser(int userId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT r.TicketCode, e.Title, e.Venue, e.EventStart, e.EventEnd, s.Name AS SocietyName, r.RegisteredAt
            FROM dbo.EventRegistrations r
            INNER JOIN dbo.Events e ON e.EventID = r.EventID
            INNER JOIN dbo.Societies s ON s.SocietyID = e.SocietyID
            WHERE r.UserID=@uid AND e.Status <> N'Cancelled'
            ORDER BY e.EventStart DESC
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@uid", userId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static DataTable GetUniversityReport()
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT s.Name AS Society, s.Status AS SocietyStatus,
                   COUNT(DISTINCT m.UserID) AS Members,
                   COUNT(DISTINCT e.EventID) AS TotalEvents,
                   SUM(CASE WHEN e.Status=N'Approved' THEN 1 ELSE 0 END) AS ApprovedEvents
            FROM dbo.Societies s
            LEFT JOIN dbo.Memberships m ON m.SocietyID=s.SocietyID AND m.Status=N'Approved'
            LEFT JOIN dbo.Events e ON e.SocietyID=s.SocietyID
            GROUP BY s.SocietyID, s.Name, s.Status
            ORDER BY s.Name
            """, cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static DataTable GetSocietyEventsReport(int societyId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT e.Title, e.Status, e.EventStart,
                   (SELECT COUNT(*) FROM dbo.EventRegistrations r WHERE r.EventID=e.EventID) AS Attendees
            FROM dbo.Events e WHERE e.SocietyID=@sid ORDER BY e.EventStart DESC
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@sid", societyId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }
}
