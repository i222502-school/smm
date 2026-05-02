using System.Data;
using Microsoft.Data.SqlClient;

namespace SocietiesManagementSystem.Data;

public static class SocietyRepository
{
    public static DataTable GetApprovedSocietiesForBrowse()
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT SocietyID, Name, Description, CreatedAt
            FROM dbo.Societies WHERE Status = N'Approved' ORDER BY Name
            """, cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static DataTable GetSocietiesForAdmin()
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT s.SocietyID, s.Name, s.Description, s.Status, s.CreatedAt,
                   u.FullName AS CreatorName
            FROM dbo.Societies s
            INNER JOIN dbo.Users u ON u.UserID = s.CreatedByUserID
            ORDER BY s.SocietyID
            """, cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    /// <summary>Create society request (Pending). Creator becomes Head upon admin approval.</summary>
    public static int CreateSocietyRequest(int creatorUserId, string name, string description)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var tx = cn.BeginTransaction();
        try
        {
            const string insSoc = """
                INSERT INTO dbo.Societies (Name, Description, Status, CreatedByUserID)
                OUTPUT INSERTED.SocietyID
                VALUES (@n, @d, N'Pending', @uid)
                """;
            int societyId;
            using (var cmd = new SqlCommand(insSoc, cn, tx))
            {
                cmd.Parameters.AddWithValue("@n", name.Trim());
                cmd.Parameters.AddWithValue("@d", description.Trim());
                cmd.Parameters.AddWithValue("@uid", creatorUserId);
                societyId = (int)cmd.ExecuteScalar()!;
            }

            const string insMem = """
                INSERT INTO dbo.Memberships (UserID, SocietyID, RoleInSociety, Status, ResolvedAt)
                VALUES (@uid, @sid, N'Head', N'Approved', SYSUTCDATETIME())
                """;
            using (var cmd = new SqlCommand(insMem, cn, tx))
            {
                cmd.Parameters.AddWithValue("@uid", creatorUserId);
                cmd.Parameters.AddWithValue("@sid", societyId);
                cmd.ExecuteNonQuery();
            }

            ActivityLogRepository.Log(cn, creatorUserId, "SocietyCreatedPending", "Society", societyId, name);
            tx.Commit();
            return societyId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static void UpdateSocietyProfile(int societyId, string name, string description, int actingUserId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            UPDATE dbo.Societies SET Name=@n, Description=@d WHERE SocietyID=@sid AND Status <> N'Rejected'
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@n", name.Trim());
        cmd.Parameters.AddWithValue("@d", description.Trim());
        cmd.Parameters.AddWithValue("@sid", societyId);
        cmd.ExecuteNonQuery();
        ActivityLogRepository.Log(cn, actingUserId, "SocietyProfileUpdated", "Society", societyId, null);
    }

    public static void AdminSetSocietyStatus(int societyId, string status, int adminUserId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            UPDATE dbo.Societies
            SET Status=@st,
                ApprovedByUserID = CASE WHEN @st = N'Approved' THEN @aid ELSE ApprovedByUserID END,
                ApprovedAt = CASE WHEN @st = N'Approved' THEN SYSUTCDATETIME() ELSE ApprovedAt END
            WHERE SocietyID=@sid
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@st", status);
        cmd.Parameters.AddWithValue("@aid", adminUserId);
        cmd.Parameters.AddWithValue("@sid", societyId);
        cmd.ExecuteNonQuery();
        ActivityLogRepository.Log(cn, adminUserId, "AdminSocietyStatus", "Society", societyId, status);
    }

    public static void AdminDeleteSociety(int societyId, int adminUserId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = "DELETE FROM dbo.Societies WHERE SocietyID=@sid";
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@sid", societyId);
        cmd.ExecuteNonQuery();
        ActivityLogRepository.Log(cn, adminUserId, "AdminDeleteSociety", "Society", societyId, null);
    }

    public static bool IsHeadOf(int userId, int societyId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            SELECT COUNT(*) FROM dbo.Memberships
            WHERE UserID=@u AND SocietyID=@s AND RoleInSociety=N'Head' AND Status=N'Approved'
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@s", societyId);
        return (int)cmd.ExecuteScalar()! > 0;
    }

    public static bool IsApprovedMemberOf(int userId, int societyId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            SELECT COUNT(*) FROM dbo.Memberships
            WHERE UserID=@u AND SocietyID=@s AND Status=N'Approved'
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@s", societyId);
        return (int)cmd.ExecuteScalar()! > 0;
    }

    public static DataTable GetHeadSocieties(int userId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT s.SocietyID, s.Name, s.Status
            FROM dbo.Societies s
            INNER JOIN dbo.Memberships m ON m.SocietyID = s.SocietyID
            WHERE m.UserID=@uid AND m.RoleInSociety=N'Head' AND m.Status=N'Approved'
              AND s.Status=N'Approved'
            ORDER BY s.Name
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@uid", userId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static DataTable GetMemberSocieties(int userId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT s.SocietyID, s.Name, m.RoleInSociety, m.Status AS MembershipStatus
            FROM dbo.Memberships m
            INNER JOIN dbo.Societies s ON s.SocietyID = m.SocietyID
            WHERE m.UserID=@uid AND s.Status IN (N'Approved', N'Suspended')
            ORDER BY s.Name
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@uid", userId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static void ApplyMembership(int userId, int societyId, string roleMember)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            MERGE dbo.Memberships AS T
            USING (SELECT @u AS UserID, @s AS SocietyID) AS S
            ON T.UserID=S.UserID AND T.SocietyID=S.SocietyID
            WHEN MATCHED AND T.Status = N'Rejected' THEN
              UPDATE SET RoleInSociety=@role, Status=N'Pending', RequestedAt=SYSUTCDATETIME(), ResolvedAt=NULL
            WHEN NOT MATCHED THEN
              INSERT (UserID, SocietyID, RoleInSociety, Status) VALUES (@u, @s, @role, N'Pending');
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@u", userId);
        cmd.Parameters.AddWithValue("@s", societyId);
        cmd.Parameters.AddWithValue("@role", roleMember);
        cmd.ExecuteNonQuery();
        ActivityLogRepository.Log(cn, userId, "MembershipApplied", "Society", societyId, roleMember);
    }

    public static DataTable GetPendingMembershipsForSociety(int societyId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT m.MembershipID, u.Username, u.FullName, m.RoleInSociety, m.RequestedAt
            FROM dbo.Memberships m
            INNER JOIN dbo.Users u ON u.UserID = m.UserID
            WHERE m.SocietyID=@sid AND m.Status=N'Pending' AND m.RoleInSociety=N'Member'
            ORDER BY m.RequestedAt
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@sid", societyId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static DataTable GetMemberRoster(int societyId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        using var da = new SqlDataAdapter(
            """
            SELECT u.UserID, u.Username, u.FullName, u.Email, m.RoleInSociety, m.Status, m.RequestedAt
            FROM dbo.Memberships m
            INNER JOIN dbo.Users u ON u.UserID = m.UserID
            WHERE m.SocietyID=@sid
            ORDER BY m.RoleInSociety DESC, u.FullName
            """, cn);
        da.SelectCommand!.Parameters.AddWithValue("@sid", societyId);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public static void ResolveMembership(int membershipId, bool approve, int headUserId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            UPDATE dbo.Memberships
            SET Status = CASE WHEN @ok=1 THEN N'Approved' ELSE N'Rejected' END,
                ResolvedAt = SYSUTCDATETIME()
            WHERE MembershipID=@mid AND RoleInSociety=N'Member'
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@ok", approve);
        cmd.Parameters.AddWithValue("@mid", membershipId);
        cmd.ExecuteNonQuery();
        ActivityLogRepository.Log(cn, headUserId, approve ? "MembershipApproved" : "MembershipRejected", "Membership", membershipId, null);
    }

    /// <summary>Head removes an approved member (not another head).</summary>
    public static void RemoveMemberFromSociety(int societyId, int memberUserId, int headUserId)
    {
        using var cn = SqlConnectionFactory.CreateOpenConnection();
        const string sql = """
            DELETE FROM dbo.Memberships
            WHERE SocietyID=@sid AND UserID=@mid AND RoleInSociety=N'Member'
            """;
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@sid", societyId);
        cmd.Parameters.AddWithValue("@mid", memberUserId);
        var n = cmd.ExecuteNonQuery();
        if (n > 0)
            ActivityLogRepository.Log(cn, headUserId, "MemberRemoved", "Society", societyId, $"User {memberUserId}");
    }
}
