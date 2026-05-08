using Xunit.Abstractions;

namespace SocietiesManagementSystem.Tests.FaultInjection;

public class FaultInjectionAdminModulesTests
{
    private readonly ITestOutputHelper _output;
    public FaultInjectionAdminModulesTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void AnnouncementPost_Module_FiveFaultsInjected()
    {
        static (bool isGlobal, string title, string body) Oracle((int? societyId, string title, string body) x)
            => (!x.societyId.HasValue, x.title.Trim(), x.body.Trim());

        var inputs = new[]
        {
            ((int?)null, "  Welcome  ", "  Orientation starts Monday  "),
            ((int?)1, "Gaming", "LAN Event"),
            ((int?)2, " Sports ", " Trials ")
        };

        var faults = new List<FaultCase<(int? societyId, string title, string body), (bool isGlobal, string title, string body)>>
        {
            new("AN_F1_GlobalFlagInverted", x => (x.societyId.HasValue, x.title.Trim(), x.body.Trim())),
            new("AN_F2_NoTrimTitle", x => (!x.societyId.HasValue, x.title, x.body.Trim())),
            new("AN_F3_NoTrimBody", x => (!x.societyId.HasValue, x.title.Trim(), x.body)),
            new("AN_F4_AlwaysGlobal", x => (true, x.title.Trim(), x.body.Trim())),
            new("AN_F5_EmptyBody", x => (!x.societyId.HasValue, x.title.Trim(), ""))
        };

        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "AnnouncementRepository.Post");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void UserAdminUpdate_Module_FiveFaultsInjected()
    {
        static (string email, string fullName, bool active) Oracle((string email, string fullName, bool active) x)
            => (x.email, x.fullName, x.active);

        var inputs = new[]
        {
            ("student1@example.com", "Ali Khan", true),
            ("inactive@example.com", "Sara", false),
            ("new@example.com", "John Doe", true)
        };

        var faults = new List<FaultCase<(string email, string fullName, bool active), (string email, string fullName, bool active)>>
        {
            new("UA_F1_DropEmailUpdate", x => ("old@example.com", x.fullName, x.active)),
            new("UA_F2_DropNameUpdate", x => (x.email, "OLD NAME", x.active)),
            new("UA_F3_FlipActiveFlag", x => (x.email, x.fullName, !x.active)),
            new("UA_F4_TrimAndLowerEmail", x => (x.email.Trim().ToLowerInvariant(), x.fullName, x.active)),
            new("UA_F5_EmptyName", x => (x.email, "", x.active))
        };

        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "UserAdminRepository.UpdateUser");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void SocietyStatusAdmin_Module_FiveFaultsInjected()
    {
        static string Oracle((string targetStatus, bool approvedAction) x)
            => x.approvedAction ? "Approved" : x.targetStatus;

        var inputs = new[]
        {
            ("Suspended", false),
            ("Rejected", false),
            ("Approved", true),
            ("Pending", false)
        };

        var faults = new List<FaultCase<(string targetStatus, bool approvedAction), string>>
        {
            new("SS_F1_AlwaysApproved", _ => "Approved"),
            new("SS_F2_AlwaysSuspended", _ => "Suspended"),
            new("SS_F3_InvertApprovalPath", x => x.approvedAction ? x.targetStatus : "Approved"),
            new("SS_F4_ForceRejected", _ => "Rejected"),
            new("SS_F5_NoChange", _ => "Pending")
        };

        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "SocietyRepository.AdminSetSocietyStatus");
        Assert.Equal(5, r.total);
    }
}

