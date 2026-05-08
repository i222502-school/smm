using Xunit.Abstractions;

namespace SocietiesManagementSystem.Tests.FaultInjection;

public class FaultInjectionAdditionalModulesTests
{
    private readonly ITestOutputHelper _output;
    public FaultInjectionAdditionalModulesTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void CsvExportHelper_Quote_Module_FiveFaultsInjected()
    {
        static string Oracle(string s) => $"\"{s.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        var inputs = new[] { "sample", "a\"b", "", " spaced " };
        var faults = new List<FaultCase<string, string>>
        {
            new("CSV_F1_NoOuterQuotes", s => s.Replace("\"", "\"\"", StringComparison.Ordinal)),
            new("CSV_F2_NoEscape", s => $"\"{s}\""),
            new("CSV_F3_SingleEscapeOnly", s => $"\"{s.Replace("\"", "'", StringComparison.Ordinal)}\""),
            new("CSV_F4_TrimInput", s => $"\"{s.Trim().Replace("\"", "\"\"", StringComparison.Ordinal)}\""),
            new("CSV_F5_AlwaysEmpty", _ => "\"\"")
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "CsvExportHelper.Quote");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void LoginFlowDecision_Module_FiveFaultsInjected()
    {
        static string Oracle((bool userExists, bool active, string userType) x)
        {
            if (!x.userExists || !x.active) return "Error";
            return x.userType == "Admin" ? "AdminDashboard" : "StudentPortal";
        }

        var inputs = new[]
        {
            (false, false, "Student"),
            (true, false, "Student"),
            (true, true, "Student"),
            (true, true, "Admin")
        };

        var faults = new List<FaultCase<(bool userExists, bool active, string userType), string>>
        {
            new("LG_F1_InactiveAllowed", x => !x.userExists ? "Error" : (x.userType == "Admin" ? "AdminDashboard" : "StudentPortal")),
            new("LG_F2_AdminAsStudent", x => (!x.userExists || !x.active) ? "Error" : "StudentPortal"),
            new("LG_F3_StudentAsAdmin", x => (!x.userExists || !x.active) ? "Error" : "AdminDashboard"),
            new("LG_F4_AlwaysError", _ => "Error"),
            new("LG_F5_UserExistsOnly", x => x.userExists ? (x.userType == "Admin" ? "AdminDashboard" : "StudentPortal") : "Error")
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "LoginForm.TryLogin flow");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void SocietyApplyMembership_Module_FiveFaultsInjected()
    {
        static string Oracle((bool existingRejected, bool existingApproved, string role) x)
        {
            if (x.existingApproved) return "NoChange";
            return "Pending:" + x.role;
        }

        var inputs = new[]
        {
            (false, false, "Member"),
            (true, false, "Member"),
            (false, true, "Member"),
            (true, false, "Head")
        };

        var faults = new List<FaultCase<(bool existingRejected, bool existingApproved, string role), string>>
        {
            new("AP_F1_AutoApprove", x => x.existingApproved ? "NoChange" : "Approved:" + x.role),
            new("AP_F2_AlwaysNoChange", _ => "NoChange"),
            new("AP_F3_ForceMemberRole", x => x.existingApproved ? "NoChange" : "Pending:Member"),
            new("AP_F4_RejectInsteadPending", x => x.existingApproved ? "NoChange" : "Rejected:" + x.role),
            new("AP_F5_InvertApprovedCheck", x => !x.existingApproved ? "NoChange" : "Pending:" + x.role)
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "SocietyRepository.ApplyMembership");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void EventCreateStatus_Module_FiveFaultsInjected()
    {
        static string Oracle((bool submitForApproval, DateTime start, DateTime end) x)
            => x.end > x.start ? (x.submitForApproval ? "PendingAdminApproval" : "Draft") : "InvalidTime";

        var now = DateTime.UtcNow;
        var inputs = new[]
        {
            (true, now, now.AddHours(2)),
            (false, now, now.AddHours(2)),
            (true, now, now.AddHours(-1))
        };

        var faults = new List<FaultCase<(bool submitForApproval, DateTime start, DateTime end), string>>
        {
            new("CE_F1_AlwaysDraft", x => x.end > x.start ? "Draft" : "InvalidTime"),
            new("CE_F2_AlwaysPending", x => x.end > x.start ? "PendingAdminApproval" : "InvalidTime"),
            new("CE_F3_IgnoreTimeValidation", x => x.submitForApproval ? "PendingAdminApproval" : "Draft"),
            new("CE_F4_InvertSubmitFlag", x => x.end > x.start ? (x.submitForApproval ? "Draft" : "PendingAdminApproval") : "InvalidTime"),
            new("CE_F5_AlwaysInvalid", _ => "InvalidTime")
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "EventRepository.CreateEvent status");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void EventAdminApproval_Module_FiveFaultsInjected()
    {
        static string Oracle((bool approve, bool currentlyPending) x)
            => !x.currentlyPending ? "NoChange" : (x.approve ? "Approved" : "Cancelled");

        var inputs = new[]
        {
            (true, true),
            (false, true),
            (true, false),
            (false, false)
        };

        var faults = new List<FaultCase<(bool approve, bool currentlyPending), string>>
        {
            new("EA_F1_RejectAsRejected", x => !x.currentlyPending ? "NoChange" : (x.approve ? "Approved" : "Rejected")),
            new("EA_F2_AlwaysApproved", x => x.currentlyPending ? "Approved" : "NoChange"),
            new("EA_F3_AlwaysCancelled", x => x.currentlyPending ? "Cancelled" : "NoChange"),
            new("EA_F4_IgnorePendingCheck", x => x.approve ? "Approved" : "Cancelled"),
            new("EA_F5_InvertDecision", x => !x.currentlyPending ? "NoChange" : (x.approve ? "Cancelled" : "Approved"))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "EventRepository.AdminApproveEvent");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void SocietyHeadCheck_Module_FiveFaultsInjected()
    {
        static bool Oracle((bool hasMembership, string role, string status) x)
            => x.hasMembership && x.role == "Head" && x.status == "Approved";

        var inputs = new[]
        {
            (true, "Head", "Approved"),
            (true, "Member", "Approved"),
            (true, "Head", "Pending"),
            (false, "Head", "Approved")
        };

        var faults = new List<FaultCase<(bool hasMembership, string role, string status), bool>>
        {
            new("HD_F1_IgnoreStatus", x => x.hasMembership && x.role == "Head"),
            new("HD_F2_IgnoreRole", x => x.hasMembership && x.status == "Approved"),
            new("HD_F3_AnyMembership", x => x.hasMembership),
            new("HD_F4_InvertResult", x => !(x.hasMembership && x.role == "Head" && x.status == "Approved")),
            new("HD_F5_AlwaysFalse", _ => false)
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "SocietyRepository.IsHeadOf");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void MemberWorkspaceRouting_Module_FiveFaultsInjected()
    {
        static string Oracle((string role, string membershipStatus) x)
        {
            if (x.membershipStatus != "Approved") return "Blocked";
            return x.role == "Head" ? "LeadershipForm" : "MemberForm";
        }

        var inputs = new[]
        {
            ("Head", "Approved"),
            ("Member", "Approved"),
            ("Member", "Pending"),
            ("Head", "Rejected")
        };

        var faults = new List<FaultCase<(string role, string membershipStatus), string>>
        {
            new("WS_F1_AllApprovedAllowed", x => x.role == "Head" ? "LeadershipForm" : "MemberForm"),
            new("WS_F2_HeadAsMember", x => x.membershipStatus != "Approved" ? "Blocked" : "MemberForm"),
            new("WS_F3_MemberAsHead", x => x.membershipStatus != "Approved" ? "Blocked" : "LeadershipForm"),
            new("WS_F4_InvertApprovalGuard", x => x.membershipStatus == "Approved" ? "Blocked" : (x.role == "Head" ? "LeadershipForm" : "MemberForm")),
            new("WS_F5_AlwaysBlocked", _ => "Blocked")
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "StudentPortalForm.OpenWorkspace flow");
        Assert.Equal(5, r.total);
    }
}

