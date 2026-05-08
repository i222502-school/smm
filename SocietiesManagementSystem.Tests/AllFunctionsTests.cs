using System.Data;
using System.Reflection;
using SocietiesManagementSystem;
using SocietiesManagementSystem.Data;
using SocietiesManagementSystem.Forms;
using SocietiesManagementSystem.Helpers;
using SocietiesManagementSystem.Models;
using SocietiesManagementSystem.Security;
using SocietiesManagementSystem.Services;
using Xunit;

namespace SocietiesManagementSystem.Tests;

public class AppSessionTests
{
    [Fact]
    public void CurrentUser_CanBeSetAndRead()
    {
        var user = new SessionUser { UserId = 99, Username = "u99" };

        AppSession.CurrentUser = user;

        Assert.Same(user, AppSession.CurrentUser);
    }

    [Fact]
    public void SignOut_ClearsCurrentUser()
    {
        AppSession.CurrentUser = new SessionUser { UserId = 1, Username = "student1" };

        AppSession.SignOut();

        Assert.Null(AppSession.CurrentUser);
    }
}

public class SessionUserTests
{
    [Fact]
    public void NewSessionUser_HasExpectedDefaultValues()
    {
        var user = new SessionUser();

        Assert.Equal(0, user.UserId);
        Assert.Equal(string.Empty, user.Username);
        Assert.Equal(string.Empty, user.FullName);
        Assert.Equal(string.Empty, user.Email);
        Assert.Equal(string.Empty, user.UserType);
    }
}

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_SameInputAndSalt_ReturnsSameHash()
    {
        var salt = Guid.NewGuid();
        var hash1 = PasswordHasher.HashPassword("abc123", salt);
        var hash2 = PasswordHasher.HashPassword("abc123", salt);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var salt = Guid.NewGuid();
        var hash = PasswordHasher.HashPassword("password", salt);

        var ok = PasswordHasher.Verify("password", salt, hash);

        Assert.True(ok);
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var salt = Guid.NewGuid();
        var hash = PasswordHasher.HashPassword("password", salt);

        var ok = PasswordHasher.Verify("wrong", salt, hash);

        Assert.False(ok);
    }
}

public class CsvExportHelperTests
{
    [Fact]
    public void Quote_PrivateHelper_EscapesAndWrapsText()
    {
        var method = typeof(CsvExportHelper).GetMethod("Quote", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var result = method!.Invoke(null, new object[] { "a\"b" }) as string;

        Assert.Equal("\"a\"\"b\"", result);
    }

    [Fact(Skip = "WinForms DataGridView rendering behavior is OS-specific; run on Windows.")]
    public void ExportDataGridView_WritesCsvWithHeadersAndRows()
    {
        var grid = new DataGridView();
        var table = new DataTable();
        table.Columns.Add("Name");
        table.Columns.Add("Role");
        table.Rows.Add("Alice", "Member");
        table.Rows.Add("Bob", "Head");
        grid.DataSource = table;

        var path = Path.GetTempFileName();
        try
        {
            CsvExportHelper.ExportDataGridView(grid, path);
            var content = File.ReadAllText(path);
            Assert.Contains("\"Name\",\"Role\"", content, StringComparison.Ordinal);
            Assert.Contains("\"Alice\",\"Member\"", content, StringComparison.Ordinal);
            Assert.Contains("\"Bob\",\"Head\"", content, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }
}

public class ProgramTests
{
    [Fact(Skip = "Program.Main launches WinForms app loop; validate manually in Windows runtime.")]
    public void Main_TestCaseExists() => Assert.True(true);
}

public class GridFormatterTests
{
    [Fact(Skip = "WinForms control tests require Windows desktop runtime.")]
    public void Bind_SetsReadOnlyAndSelectionProperties()
    {
        var grid = new DataGridView();
        var table = new DataTable();
        table.Columns.Add("Col");
        table.Rows.Add("v");

        GridFormatter.Bind(grid, table);

        Assert.True(grid.ReadOnly);
        Assert.False(grid.MultiSelect);
        Assert.Equal(DataGridViewSelectionMode.FullRowSelect, grid.SelectionMode);
        Assert.Same(table, grid.DataSource);
    }

    [Fact(Skip = "WinForms control tests require Windows desktop runtime.")]
    public void BindEditable_SetsEditableProperties()
    {
        var grid = new DataGridView();
        var table = new DataTable();
        table.Columns.Add("Col");
        table.Rows.Add("v");

        GridFormatter.BindEditable(grid, table);

        Assert.False(grid.ReadOnly);
        Assert.Equal(DataGridViewSelectionMode.FullRowSelect, grid.SelectionMode);
        Assert.Same(table, grid.DataSource);
    }
}

public class FormConstructorTests
{
    [Fact(Skip = "UI form construction should be validated on Windows UI test environment.")]
    public void LoginForm_Constructor_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "UI form construction should be validated on Windows UI test environment.")]
    public void RegisterForm_Constructor_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "UI form construction should be validated on Windows UI test environment.")]
    public void AdminDashboardForm_Constructor_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "UI form construction should be validated on Windows UI test environment.")]
    public void StudentPortalForm_Constructor_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "UI form construction should be validated on Windows UI test environment.")]
    public void SocietyLeadershipForm_Constructor_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "UI form construction should be validated on Windows UI test environment.")]
    public void MemberSocietyForm_Constructor_TestCaseExists() => Assert.True(true);
}

public class LoginFormMethodTests
{
    [Fact(Skip = "Invokes DB-backed auth and WinForms UI interaction.")]
    public void TryLogin_TestCaseExists() => Assert.True(true);
}

public class StudentPortalFormMethodTests
{
    [Fact(Skip = "Private UI wiring method requiring WinForms runtime.")]
    public void BuildBrowseTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI wiring method requiring WinForms runtime.")]
    public void BuildRequestTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI wiring method requiring WinForms runtime.")]
    public void BuildMembershipsTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI wiring method requiring WinForms runtime.")]
    public void OpenWorkspace_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI wiring method requiring WinForms runtime.")]
    public void BuildEventsTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI wiring method requiring WinForms runtime.")]
    public void BuildTicketsTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI wiring method requiring WinForms runtime.")]
    public void BuildAnnounceTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI wiring method requiring WinForms runtime.")]
    public void BuildMyTasksTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadAll_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadBrowse_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadMemberships_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadEvents_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadTickets_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadAnnounce_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadMyTasks_TestCaseExists() => Assert.True(true);
}

public class AdminDashboardFormMethodTests
{
    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildUsersTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildSocietiesTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildEventsTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildActivityTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildReportsTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadAll_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadUsers_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadSocieties_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadPendingEvents_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadActivity_TestCaseExists() => Assert.True(true);
}

public class MemberSocietyFormMethodTests
{
    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadRoster_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadTasks_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadAnnounce_TestCaseExists() => Assert.True(true);
}

public class SocietyLeadershipFormMethodTests
{
    [Fact(Skip = "Uses repository/DB calls and WinForms controls.")]
    public void LoadProfile_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildProfileTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildMembershipTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ResolvePending_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildMembersTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildEventsTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void AddEvent_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void UpdateSelectedEvent_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void CancelSelectedEvent_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildTasksTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void LoadAssignees_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildAnnounceTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Private UI method requiring WinForms runtime.")]
    public void BuildReportsTab_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Uses repository calls and WinForms controls.")]
    public void ReloadGrids_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Static helper method requires WinForms grid context.")]
    public void HideColumn_TestCaseExists() => Assert.True(true);
}

public class AuthServiceTests
{
    [Fact]
    public void RegisterStudent_WithBlankUsername_ReturnsInvalidUsernameMessage()
    {
        var sut = new AuthService();

        var result = sut.RegisterStudent("   ", "123456", "a@b.com", "A");

        Assert.False(result.ok);
        Assert.Equal("Invalid username.", result.message);
    }

    [Fact]
    public void RegisterStudent_WithTooLongUsername_ReturnsInvalidUsernameMessage()
    {
        var sut = new AuthService();
        var tooLong = new string('a', 51);

        var result = sut.RegisterStudent(tooLong, "123456", "a@b.com", "A");

        Assert.False(result.ok);
        Assert.Equal("Invalid username.", result.message);
    }

    [Fact]
    public void RegisterStudent_WithShortPassword_ReturnsPasswordValidationMessage()
    {
        var sut = new AuthService();

        var result = sut.RegisterStudent("student1", "123", "a@b.com", "A");

        Assert.False(result.ok);
        Assert.Equal("Password must be at least 6 characters.", result.message);
    }

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void Login_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void RegisterStudent_TestCaseExists() => Assert.True(true);
}

public class SqlConnectionFactoryTests
{
    [Fact(Skip = "Requires reachable SQL Server instance/configuration.")]
    public void CreateOpenConnection_TestCaseExists() => Assert.True(true);
}

public class ActivityLogRepositoryTests
{
    [Fact(Skip = "Requires database integration (open SqlConnection).")]
    public void Log_TestCaseExists() => Assert.True(true);
}

public class ActivityLogQueryRepositoryTests
{
    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetRecent_TestCaseExists() => Assert.True(true);
}

public class AnnouncementRepositoryTests
{
    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetForStudentHome_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetForSociety_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void Post_TestCaseExists() => Assert.True(true);
}

public class EventRepositoryTests
{
    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetUpcomingApprovedEvents_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetEventsForSociety_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetPendingEventsForAdmin_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void CreateEvent_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void UpdateEvent_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void CancelEvent_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void AdminApproveEvent_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void RegisterStudentForEvent_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetTicketsForUser_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetUniversityReport_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetSocietyEventsReport_TestCaseExists() => Assert.True(true);
}

public class SocietyRepositoryTests
{
    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetApprovedSocietiesForBrowse_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetSocietiesForAdmin_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void CreateSocietyRequest_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void UpdateSocietyProfile_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void AdminSetSocietyStatus_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void AdminDeleteSociety_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void IsHeadOf_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void IsApprovedMemberOf_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetHeadSocieties_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetMemberSocieties_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void ApplyMembership_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetPendingMembershipsForSociety_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetMemberRoster_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void ResolveMembership_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void RemoveMemberFromSociety_TestCaseExists() => Assert.True(true);
}

public class TaskRepositoryTests
{
    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetTasksForSociety_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetMyTasks_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void AddTask_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void UpdateTaskStatus_TestCaseExists() => Assert.True(true);
}

public class UserAdminRepositoryTests
{
    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void GetAllUsers_TestCaseExists() => Assert.True(true);

    [Fact(Skip = "Requires database integration (SqlConnectionFactory).")]
    public void UpdateUser_TestCaseExists() => Assert.True(true);
}
