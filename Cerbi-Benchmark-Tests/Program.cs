using BenchmarkDotNet.Running;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(">>> Running updated PopularLoggerBenchmarks (subset for quick validation)...");

        // For quicker iteration you can filter benchmarks via BenchmarkSwitcher or regex; left simple here
        BenchmarkRunner.Run<CerbiBenchmark.PopularLoggerBenchmarks>();
    }
}
