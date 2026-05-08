using Xunit.Abstractions;

namespace SocietiesManagementSystem.Tests.FaultInjection;

public sealed record FaultCase<TInput, TOutput>(string Id, Func<TInput, TOutput> Mutant);

public static class FaultInjectionHarness
{
    public static (int detected, int total, double probabilityNoMoreThanOne) Evaluate<TInput, TOutput>(
        IEnumerable<TInput> cyclomaticInputs,
        Func<TInput, TOutput> oracle,
        IReadOnlyList<FaultCase<TInput, TOutput>> faults,
        ITestOutputHelper output,
        string moduleName)
    {
        int detected = 0;
        foreach (var fault in faults)
        {
            bool mismatchFound = false;
            foreach (var input in cyclomaticInputs)
            {
                var expected = oracle(input);
                var actual = fault.Mutant(input);
                if (!EqualityComparer<TOutput>.Default.Equals(expected, actual))
                {
                    mismatchFound = true;
                    break;
                }
            }

            if (mismatchFound) detected++;
            output.WriteLine($"{moduleName} :: {fault.Id} => {(mismatchFound ? "DETECTED" : "UNDETECTED")}");
        }

        const int n = 5;
        var p = detected / (double)n;
        var probabilityNoMoreThanOne = Math.Pow(p, 5) + 5 * (1 - p) * Math.Pow(p, 4);
        output.WriteLine($"{moduleName} => detected={detected}/{n}, P(<=1 fault)={probabilityNoMoreThanOne:F4}");
        return (detected, n, probabilityNoMoreThanOne);
    }
}

