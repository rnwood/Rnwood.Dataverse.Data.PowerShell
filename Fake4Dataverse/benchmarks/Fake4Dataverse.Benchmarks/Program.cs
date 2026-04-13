using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Fake4Dataverse.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // FakeXrmEasy packages ship without a Release build flag — disable the
            // optimisation validator so BenchmarkDotNet accepts them as dependencies.
            //
            // InProcessEmit avoids spawning child processes (some AV software blocks
            // those on Windows), while still supporting MemoryDiagnoser.
            var config = DefaultConfig.Instance
                .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                .AddJob(Job.ShortRun.WithToolchain(InProcessEmitToolchain.Instance));

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
