using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CerbiStream.Logging;
using CerbiStream.Configuration;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System.Reflection;
using System.Text;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Log4NetManager = log4net.LogManager;
using NLogManager = NLog.LogManager;

namespace CerbiBenchmark
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class PopularLoggerBenchmarks
    {
        private ILogger _msLoggerPlain, _msLoggerEncrypted;
        private ILogger _serilogPlain, _serilogEncrypted;
        private ILogger _nlogPlain, _nlogEncrypted;
        private ILogger _log4netPlain, _log4netEncrypted;
        private ILogger _cerbiPlain, _cerbiEncrypted;

        private string EncryptBase64(string input) => Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

        [GlobalSetup]
        public void Setup()
        {
            Console.WriteLine(">>> Setup initialized.");

            _msLoggerPlain = BuildLogger(s => s.AddLogging(b => b.AddConsole()));
            _msLoggerEncrypted = _msLoggerPlain;

            _serilogPlain = BuildLogger(s =>
            {
                var serilog = new LoggerConfiguration().WriteTo.Console().CreateLogger();
                s.AddLogging(b => b.AddSerilog(serilog));
            });
            _serilogEncrypted = _serilogPlain;

            _nlogPlain = BuildLogger(s =>
            {
                var _ = NLogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
                s.AddLogging(b => b.AddNLog());
            });
            _nlogEncrypted = _nlogPlain;

            _log4netPlain = BuildLogger(s =>
            {
                ILoggerRepository repo = Log4NetManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
                XmlConfigurator.Configure(repo, new FileInfo("log4net.config"));
                s.AddLogging(b => b.AddLog4Net("log4net.config"));
            });
            _log4netEncrypted = _log4netPlain;

            _cerbiPlain = BuildLogger(s =>
            {
                s.AddLogging(b => b.AddCerbiStream(opt =>
                {
                    opt.EnableDevModeMinimal();
                }));
            });

            _cerbiEncrypted = BuildLogger(s =>
            {
                s.AddLogging(b => b.AddCerbiStream(opt =>
                {
                    opt.EnableDevModeMinimal();
                    opt.WithEncryptionMode(CerbiStream.Interfaces.IEncryptionTypeProvider.EncryptionType.Base64);
                }));
            });
        }

        private ILogger BuildLogger(Action<ServiceCollection> configure)
        {
            var services = new ServiceCollection();
            configure(services);
            return services.BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Benchmark");
        }

        [Benchmark(Baseline = true)]
        public void MS_Log_Plain() => _msLoggerPlain.LogInformation("MS Logger: Logging at {time}", DateTime.UtcNow);

        [Benchmark]
        public void MS_Log_Encrypted() =>
            _msLoggerEncrypted.LogInformation(EncryptBase64($"MS Logger: Logging at {DateTime.UtcNow}"));

        [Benchmark]
        public void Serilog_Log_Plain() =>
            _serilogPlain.LogInformation("Serilog: Logging at {time}", DateTime.UtcNow);

        [Benchmark]
        public void Serilog_Log_Encrypted() =>
            _serilogEncrypted.LogInformation(EncryptBase64($"Serilog: Logging at {DateTime.UtcNow}"));

        [Benchmark]
        public void NLog_Log_Plain() =>
            _nlogPlain.LogInformation("NLog: Logging at {time}", DateTime.UtcNow);

        [Benchmark]
        public void NLog_Log_Encrypted() =>
            _nlogEncrypted.LogInformation(EncryptBase64($"NLog: Logging at {DateTime.UtcNow}"));

        [Benchmark]
        public void Log4Net_Log_Plain() =>
            _log4netPlain.LogInformation("log4net: Logging at {time}", DateTime.UtcNow);

        [Benchmark]
        public void Log4Net_Log_Encrypted() =>
            _log4netEncrypted.LogInformation(EncryptBase64($"log4net: Logging at {DateTime.UtcNow}"));

        [Benchmark]
        public void Cerbi_Log_Plain() =>
            _cerbiPlain.LogInformation("CerbiStream: Logging at {time}", DateTime.UtcNow);

        [Benchmark]
        public void Cerbi_Log_Encrypted() =>
            _cerbiEncrypted.LogInformation("CerbiStream: Logging at {time}", DateTime.UtcNow);
    }
}
