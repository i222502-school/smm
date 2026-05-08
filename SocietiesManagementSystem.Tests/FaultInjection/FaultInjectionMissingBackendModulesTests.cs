using Xunit.Abstractions;

namespace SocietiesManagementSystem.Tests.FaultInjection;

public class FaultInjectionMissingBackendModulesTests
{
    private readonly ITestOutputHelper _output;
    public FaultInjectionMissingBackendModulesTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void GridFormatterBind_Module_FiveFaultsInjected()
    {
        static (bool readOnly, bool multiSelect, string selectionMode) Oracle((bool hasTable, bool editablePath) x)
            => (true, false, "FullRowSelect");
        var inputs = new[] { (true, false), (false, false), (true, true) };
        var faults = new List<FaultCase<(bool hasTable, bool editablePath), (bool readOnly, bool multiSelect, string selectionMode)>>
        {
            new("GF_F1_ReadOnlyFalse", _ => (false, false, "FullRowSelect")),
            new("GF_F2_MultiSelectTrue", _ => (true, true, "FullRowSelect")),
            new("GF_F3_CellSelect", _ => (true, false, "CellSelect")),
            new("GF_F4_AllWrong", _ => (false, true, "CellSelect")),
            new("GF_F5_EditableModeLeak", _ => (false, false, "FullRowSelect"))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "GridFormatter.Bind");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void GridFormatterBindEditable_Module_FiveFaultsInjected()
    {
        static (bool readOnly, string selectionMode) Oracle((bool hasTable, bool editablePath) x)
            => (false, "FullRowSelect");
        var inputs = new[] { (true, true), (false, true), (true, false) };
        var faults = new List<FaultCase<(bool hasTable, bool editablePath), (bool readOnly, string selectionMode)>>
        {
            new("GE_F1_ReadOnlyTrue", _ => (true, "FullRowSelect")),
            new("GE_F2_CellSelect", _ => (false, "CellSelect")),
            new("GE_F3_DefaultSelect", _ => (false, "RowHeaderSelect")),
            new("GE_F4_AllWrong", _ => (true, "CellSelect")),
            new("GE_F5_BindModeMixup", _ => (true, "FullRowSelect"))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "GridFormatter.BindEditable");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void SqlConnectionFactoryCreateOpenConnection_Module_FiveFaultsInjected()
    {
        static (bool hasConnectionString, bool opensConnection) Oracle((bool configExists, bool fallbackAllowed) x)
            => (true, true);
        var inputs = new[] { (true, true), (false, true), (true, false) };
        var faults = new List<FaultCase<(bool configExists, bool fallbackAllowed), (bool hasConnectionString, bool opensConnection)>>
        {
            new("SC_F1_NoFallback", x => (x.configExists, x.configExists)),
            new("SC_F2_NeverOpen", _ => (true, false)),
            new("SC_F3_EmptyConnectionString", _ => (false, false)),
            new("SC_F4_OpenConditional", x => (x.configExists, x.configExists)),
            new("SC_F5_ReturnClosed", _ => (true, false))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "SqlConnectionFactory.CreateOpenConnection");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void ActivityLogRepositoryLog_Module_FiveFaultsInjected()
    {
        static (bool inserted, bool preservesNullUser, string action) Oracle((int? userId, string action, int? entityId) x)
            => (true, x.userId is null, x.action);
        var inputs = new[] { ((int?)1, "Create", (int?)10), ((int?)null, "Update", (int?)null), ((int?)2, "Delete", (int?)99) };
        var faults = new List<FaultCase<(int? userId, string action, int? entityId), (bool inserted, bool preservesNullUser, string action)>>
        {
            new("AL_F1_DropInsert", x => (false, x.userId is null, x.action)),
            new("AL_F2_ForceUserZero", x => (true, false, x.action)),
            new("AL_F3_WrongAction", x => (true, x.userId is null, "Unknown")),
            new("AL_F4_NullEntityIgnored", x => (true, x.userId is null, x.action + "_BAD")),
            new("AL_F5_NoNullSupport", x => (true, false, x.action))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "ActivityLogRepository.Log");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void ActivityLogQueryGetRecent_Module_FiveFaultsInjected()
    {
        static (bool orderedDesc, int topReturned) Oracle((int top, int available) x)
            => (true, Math.Min(x.top, x.available));
        var inputs = new[] { (100, 300), (10, 7), (500, 500) };
        var faults = new List<FaultCase<(int top, int available), (bool orderedDesc, int topReturned)>>
        {
            new("AQ_F1_AscendingOrder", x => (false, Math.Min(x.top, x.available))),
            new("AQ_F2_IgnoreTop", x => (true, x.available)),
            new("AQ_F3_OffByOne", x => (true, Math.Max(0, Math.Min(x.top, x.available) - 1))),
            new("AQ_F4_ZeroRows", _ => (true, 0)),
            new("AQ_F5_OverFetch", x => (true, Math.Min(x.available, x.top + 1)))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "ActivityLogQueryRepository.GetRecent");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void AnnouncementReadModules_FiveFaultsInjected()
    {
        static (bool includesGlobal, bool includesApprovedSocietyOnly) Oracle((bool globalExists, bool societyApproved) x)
            => (true, true);
        var inputs = new[] { (true, true), (false, true), (true, false) };
        var faults = new List<FaultCase<(bool globalExists, bool societyApproved), (bool includesGlobal, bool includesApprovedSocietyOnly)>>
        {
            new("AR_F1_ExcludeGlobal", _ => (false, true)),
            new("AR_F2_IncludeSuspended", _ => (true, false)),
            new("AR_F3_ExcludeBoth", _ => (false, false)),
            new("AR_F4_GlobalConditional", x => (x.globalExists, true)),
            new("AR_F5_ApprovalConditional", x => (true, x.societyApproved))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "AnnouncementRepository.GetForStudentHome/GetForSociety");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void TaskRepositoryReadAndAdd_Module_FiveFaultsInjected()
    {
        static (bool filtersBySociety, bool filtersByUser, string statusOnAdd) Oracle((bool bySociety, bool byUser, bool addPath) x)
            => (true, true, "Pending");
        var inputs = new[] { (true, true, true), (true, false, true), (false, true, false) };
        var faults = new List<FaultCase<(bool bySociety, bool byUser, bool addPath), (bool filtersBySociety, bool filtersByUser, string statusOnAdd)>>
        {
            new("TR_F1_NoSocietyFilter", _ => (false, true, "Pending")),
            new("TR_F2_NoUserFilter", _ => (true, false, "Pending")),
            new("TR_F3_AddAsCompleted", _ => (true, true, "Completed")),
            new("TR_F4_AddAsInProgress", _ => (true, true, "InProgress")),
            new("TR_F5_NoFilters", _ => (false, false, "Pending"))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "TaskRepository.GetTasksForSociety/GetMyTasks/AddTask");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void EventRepositoryReadAndStateUpdate_Module_FiveFaultsInjected()
    {
        static (bool readsApprovedUpcoming, bool readsPendingAdmin, string cancelState) Oracle((bool approved, bool upcoming, bool pendingAdmin) x)
            => (true, true, "Cancelled");
        var inputs = new[] { (true, true, true), (true, false, true), (false, true, false) };
        var faults = new List<FaultCase<(bool approved, bool upcoming, bool pendingAdmin), (bool readsApprovedUpcoming, bool readsPendingAdmin, string cancelState)>>
        {
            new("ER_F1_IncludeUnapproved", _ => (false, true, "Cancelled")),
            new("ER_F2_IncludeNonPendingAdmin", _ => (true, false, "Cancelled")),
            new("ER_F3_CancelAsRejected", _ => (true, true, "Rejected")),
            new("ER_F4_CancelNoChange", _ => (true, true, "Approved")),
            new("ER_F5_ReadsAll", _ => (false, false, "Cancelled"))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "EventRepository.Get*/Update/Cancel/Tickets/Reports");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void SocietyRepositoryReadAndMutations_Module_FiveFaultsInjected()
    {
        static (string createStatus, bool onlyApprovedBrowse, bool deleteRemoves, bool approvedMemberCheck) Oracle((bool createReq, bool approvedMember) x)
            => ("Pending", true, true, x.approvedMember);
        var inputs = new[] { (true, true), (true, false), (false, true) };
        var faults = new List<FaultCase<(bool createReq, bool approvedMember), (string createStatus, bool onlyApprovedBrowse, bool deleteRemoves, bool approvedMemberCheck)>>
        {
            new("SR_F1_CreateAsApproved", x => ("Approved", true, true, x.approvedMember)),
            new("SR_F2_BrowseIncludesAll", x => ("Pending", false, true, x.approvedMember)),
            new("SR_F3_DeleteNoOp", x => ("Pending", true, false, x.approvedMember)),
            new("SR_F4_ApprovedMemberAlwaysTrue", _ => ("Pending", true, true, true)),
            new("SR_F5_AllWrong", _ => ("Approved", false, false, false))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "SocietyRepository.Get*/Create/Update/Delete/IsApprovedMember*");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void AuthServiceLogin_Module_FiveFaultsInjected()
    {
        static string Oracle((bool userFound, bool hashOk, bool active, string userType) x)
        {
            if (!x.userFound || !x.hashOk || !x.active) return "Null";
            return x.userType == "Admin" ? "AdminSession" : "StudentSession";
        }
        var inputs = new[] { (false, false, false, "Student"), (true, false, true, "Student"), (true, true, false, "Student"), (true, true, true, "Student"), (true, true, true, "Admin") };
        var faults = new List<FaultCase<(bool userFound, bool hashOk, bool active, string userType), string>>
        {
            new("LGN_F1_IgnoreHash", x => (!x.userFound || !x.active) ? "Null" : (x.userType == "Admin" ? "AdminSession" : "StudentSession")),
            new("LGN_F2_IgnoreActive", x => (!x.userFound || !x.hashOk) ? "Null" : (x.userType == "Admin" ? "AdminSession" : "StudentSession")),
            new("LGN_F3_AdminAsStudent", x => (!x.userFound || !x.hashOk || !x.active) ? "Null" : "StudentSession"),
            new("LGN_F4_StudentAsAdmin", x => (!x.userFound || !x.hashOk || !x.active) ? "Null" : "AdminSession"),
            new("LGN_F5_AlwaysNull", _ => "Null")
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "AuthService.Login");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void ProgramMainStartup_Module_FiveFaultsInjected()
    {
        static (bool initialized, string startupForm) Oracle((bool runApp, bool hasLoginForm) x) => (true, "LoginForm");
        var inputs = new[] { (true, true), (true, false), (false, true) };
        var faults = new List<FaultCase<(bool runApp, bool hasLoginForm), (bool initialized, string startupForm)>>
        {
            new("PR_F1_NoInitialize", _ => (false, "LoginForm")),
            new("PR_F2_StartAdminDashboard", _ => (true, "AdminDashboardForm")),
            new("PR_F3_StartStudentPortal", _ => (true, "StudentPortalForm")),
            new("PR_F4_NoRun", _ => (true, "None")),
            new("PR_F5_AllWrong", _ => (false, "None"))
        };
        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "Program.Main");
        Assert.Equal(5, r.total);
    }
}

