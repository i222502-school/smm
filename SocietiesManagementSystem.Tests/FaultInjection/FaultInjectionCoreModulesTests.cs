using Xunit.Abstractions;

namespace SocietiesManagementSystem.Tests.FaultInjection;

public class FaultInjectionCoreModulesTests
{
    private readonly ITestOutputHelper _output;
    public FaultInjectionCoreModulesTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void PasswordHasher_Module_FiveFaultsInjected()
    {
        static int Oracle((string password, Guid salt) x)
            => HashCode.Combine(x.password.Trim(), x.salt.ToString("D").ToUpperInvariant());

        var inputs = new[]
        {
            ("Pass123!", Guid.Parse("550e8400-e29b-41d4-a716-446655440000")),
            (" Admin@123 ", Guid.Parse("11111111-1111-1111-1111-111111111111")),
            ("student", Guid.Parse("22222222-2222-2222-2222-222222222222"))
        };

        var faults = new List<FaultCase<(string password, Guid salt), int>>
        {
            new("PH_F1_IgnoreSalt", x => HashCode.Combine(x.password.Trim(), "CONST")),
            new("PH_F2_LowercaseSalt", x => HashCode.Combine(x.password.Trim(), x.salt.ToString("D").ToLowerInvariant())),
            new("PH_F3_NoTrim", x => HashCode.Combine(x.password, x.salt.ToString("D").ToUpperInvariant())),
            new("PH_F4_UseOnlySalt", x => HashCode.Combine("", x.salt.ToString("D").ToUpperInvariant())),
            new("PH_F5_UseOnlyPassword", x => HashCode.Combine(x.password.Trim(), ""))
        };

        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "PasswordHasher");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void AuthService_RegisterValidation_Module_FiveFaultsInjected()
    {
        static string Oracle((string username, string password) x)
        {
            if (string.IsNullOrWhiteSpace(x.username) || x.username.Length > 50) return "Invalid username.";
            if (x.password.Length < 6) return "Password must be at least 6 characters.";
            return "OK";
        }

        var inputs = new[]
        {
            ("", "123456"),
            ("validuser", "123"),
            ("validuser", "123456"),
            (new string('a', 51), "abcdef"),
            (" spaced ", "abcdef")
        };

        var faults = new List<FaultCase<(string username, string password), string>>
        {
            new("AU_F1_AllowBlankUsername", x => x.password.Length < 6 ? "Password must be at least 6 characters." : "OK"),
            new("AU_F2_MaxLen60", x => (string.IsNullOrWhiteSpace(x.username) || x.username.Length > 60) ? "Invalid username." : (x.password.Length < 6 ? "Password must be at least 6 characters." : "OK")),
            new("AU_F3_MinPassword4", x => (string.IsNullOrWhiteSpace(x.username) || x.username.Length > 50) ? "Invalid username." : (x.password.Length < 4 ? "Password must be at least 6 characters." : "OK")),
            new("AU_F4_WrongErrorPriority", x => x.password.Length < 6 ? "Password must be at least 6 characters." : ((string.IsNullOrWhiteSpace(x.username) || x.username.Length > 50) ? "Invalid username." : "OK")),
            new("AU_F5_TrimNotHandled", x => (x.username == "" || x.username.Length > 50) ? "Invalid username." : (x.password.Length < 6 ? "Password must be at least 6 characters." : "OK"))
        };

        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "AuthService.RegisterStudent");
        Assert.Equal(5, r.total);
    }

    [Fact]
    public void AppSession_Module_FiveFaultsInjected()
    {
        static int Oracle((int current, bool signOut) x) => x.signOut ? 0 : x.current;

        var inputs = new[]
        {
            (1, true),
            (42, true),
            (42, false),
            (0, true)
        };

        var faults = new List<FaultCase<(int current, bool signOut), int>>
        {
            new("AS_F1_SignOutNoOp", x => x.current),
            new("AS_F2_SignOutMinusOne", x => x.signOut ? -1 : x.current),
            new("AS_F3_AlwaysZero", _ => 0),
            new("AS_F4_AlwaysKeep", x => x.current),
            new("AS_F5_InvertFlag", x => x.signOut ? x.current : 0)
        };

        var r = FaultInjectionHarness.Evaluate(inputs, Oracle, faults, _output, "AppSession.SignOut");
        Assert.Equal(5, r.total);
    }
}

