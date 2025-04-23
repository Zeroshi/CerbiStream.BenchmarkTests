using BenchmarkDotNet.Running;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(">>> Running updated PopularLoggerBenchmarks...");

        BenchmarkRunner.Run<CerbiBenchmark.PopularLoggerBenchmarks>();

    }
}
