using Xunit.Abstractions;

namespace SocietiesManagementSystem.Tests.FaultInjection;

public class FaultInjectionWorkflowModulesTests
{
    private readonly ITestOutputHelper _output;
    public FaultInjectionWorkflowModulesTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void EventRegistration_Module_FiveFaultsInjected()
    {
        static string Oracle((bool approved, int? max, int registered, bool duplicate) x)
        {
            if (!x.approved) return "Event is not open for registration.";
            if (x.max.HasValue && x.registered >= x.max.Value) return "Event is full.";
            if (x.duplicate) return "You are already registered.";
            return "Registered successfully.";
        }

        var inputs = new[]
        {
            (false, (int?)100, 10, false),
            (true, (int?)2, 2, false),
            (true, (int?)10, 5, true),
            (true, (int?)10, 5, false),
            (true, (int?)null, 999, false)
        };

        var faults = new List<FaultCase<(bool approved, int? max, int registered, bool duplicate), string>>
        {
            new("EV_F1_IgnoreApproval", x => (x.max.HasValue && x.registered >= x.max.Value) ? "Event is full." : (x.duplicate ? "You are already registered." : "Registered successfully.")),
            new("EV_F2_StrictGreaterOnly", x => !x.approved ? "Event is not open for registration." : (x.max.HasValue && x.registered > x.max.Value ? "Event is full." : (x.duplicate ? "You are already registered." : "Registered successfully."))),
            new("EV_F3_DuplicateIgnored", x => !x.approved ? "Event is not open for registration." : (x.max.HasValue && x.registered >= x.max.Value ? "Event is full." : "Registered successfully.")),
            new("EV_F4_CheckDuplicateFirst", x => x.duplicate ? "You are already registered." : (!x.approved ? "Event is not open for registration." : (x.max.HasValue && x.registered >= x.max.Value ? "Event is full." : "Registered successfully."))),
            new("EV_F5_InvertFullCheck", x => !x.approved ? "Event is not open for registration." : (x.max.HasValue && x.registered < x.max.Value ? "Event is full." : (x.duplicate ? "You are already registered." : "Registered successfully.")))
        };

        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "EventRepository.RegisterStudentForEvent");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void MembershipResolution_Module_FiveFaultsInjected()
    {
        static string Oracle((bool approve, bool isMemberRole) x)
            => x.isMemberRole ? (x.approve ? "Approved" : "Rejected") : "NotApplicable";

        var inputs = new[]
        {
            (true, true),
            (false, true),
            (true, false)
        };

        var faults = new List<FaultCase<(bool approve, bool isMemberRole), string>>
        {
            new("MB_F1_AlwaysApproved", x => x.isMemberRole ? "Approved" : "NotApplicable"),
            new("MB_F2_AlwaysRejected", x => x.isMemberRole ? "Rejected" : "NotApplicable"),
            new("MB_F3_InvertApproveFlag", x => x.isMemberRole ? (x.approve ? "Rejected" : "Approved") : "NotApplicable"),
            new("MB_F4_IgnoreRoleFilter", x => x.approve ? "Approved" : "Rejected"),
            new("MB_F5_NullResolution", _ => "Pending")
        };

        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "SocietyRepository.ResolveMembership");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void TaskStatusUpdate_Module_FiveFaultsInjected()
    {
        static string Oracle((string existing, string requested, bool authorized) x)
            => x.authorized ? x.requested : x.existing;

        var inputs = new[]
        {
            ("Pending", "InProgress", true),
            ("InProgress", "Completed", true),
            ("Pending", "Completed", false),
            ("Completed", "Pending", false)
        };

        var faults = new List<FaultCase<(string existing, string requested, bool authorized), string>>
        {
            new("TK_F1_IgnoreAuthorization", x => x.requested),
            new("TK_F2_AuthorizationInverted", x => x.authorized ? x.existing : x.requested),
            new("TK_F3_ForceCompleted", x => x.authorized ? "Completed" : x.existing),
            new("TK_F4_AlwaysExisting", x => x.existing),
            new("TK_F5_AlwaysPending", _ => "Pending")
        };

        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "TaskRepository.UpdateTaskStatus");
        Assert.Equal(5, r.total);
    }
}

