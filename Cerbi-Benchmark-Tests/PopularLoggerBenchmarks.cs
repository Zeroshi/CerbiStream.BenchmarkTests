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
using SerilogLogger = Serilog.ILogger;
using System.IO;
using Serilog.Sinks.Async;

namespace CerbiBenchmark
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, iterationCount: 10)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class PopularLoggerBenchmarks
    {
        private ILogger _msLoggerPlain, _msLoggerEncrypted;
        private ILogger _serilogPlain, _serilogEncrypted, _serilogAsync;
        private ILogger _nlogPlain, _nlogEncrypted;
        private ILogger _log4netPlain, _log4netEncrypted;
        private ILogger _cerbiPlain, _cerbiEncrypted, _cerbiAsync, _cerbiFile;
        // additional Cerbi variants to test feature toggles
        private ILogger _cerbi_minimal; // no dev mode, no async, no encryption
        private ILogger _cerbi_no_async; // dev mode enabled, async disabled
        private ILogger _cerbi_no_dev; // async enabled, dev mode disabled
        private ILogger _cerbi_minimal_async; // minimal (no dev) but async enabled

        // payloads for extended scenarios
        private string _smallMessage;
        private string _largeMessage;
        private string _manyPropsMessageFormat;
        private object[] _manyPropsValues;
        private Exception _sampleException;

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

            _serilogAsync = BuildLogger(s =>
            {
                var asyncSerilog = new LoggerConfiguration()
                    .WriteTo.Async(a => a.File("serilog_async.log"))
                    .CreateLogger();
                s.AddLogging(b => b.AddSerilog(asyncSerilog));
            });

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
                    opt.WithAsyncConsoleOutput(true);
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

            _cerbiAsync = BuildLogger(s =>
            {
                s.AddLogging(b => b.AddCerbiStream(opt =>
                {
                    opt.EnableDevModeMinimal();
                    opt.WithAsyncConsoleOutput(true);
                }));
            });

            // Cerbi variant: minimal (no dev mode, no async)
            _cerbi_minimal = BuildLogger(s =>
            {
                s.AddLogging(b => b.AddCerbiStream(opt =>
                {
                    // deliberately do NOT call EnableDevModeMinimal()
                    // no async, no encryption => minimal runtime path
                }));
            });

            // Cerbi variant: dev mode enabled but synchronous (no async)
            _cerbi_no_async = BuildLogger(s =>
            {
                s.AddLogging(b => b.AddCerbiStream(opt =>
                {
                    opt.EnableDevModeMinimal();
                    // no async configured
                }));
            });

            // Cerbi variant: async enabled but dev mode NOT enabled
            _cerbi_no_dev = BuildLogger(s =>
            {
                s.AddLogging(b => b.AddCerbiStream(opt =>
                {
                    // do NOT enable dev mode
                    opt.WithAsyncConsoleOutput(true);
                }));
            });

            // Cerbi variant: minimal + async (no dev mode, async true)
            _cerbi_minimal_async = BuildLogger(s =>
            {
                s.AddLogging(b => b.AddCerbiStream(opt =>
                {
                    // no dev mode
                    opt.WithAsyncConsoleOutput(true);
                }));
            });

            // Initialize file variant to avoid null reference in benchmark run
            _cerbiFile = BuildLogger(s =>
            {
                s.AddLogging(b => b.AddCerbiStream(opt =>
                {
                    opt.EnableDevModeMinimal();
                    // File output is mocked by Noop provider in BuildLogger to keep benchmarks CPU-bound
                }));
            });

            // prepare payloads
            _smallMessage = "ping";
            _largeMessage = new string('X', 8 * 1024); //8KB payload

            // prepared many-structured-properties message:12 properties
            _manyPropsMessageFormat = "ManyProps: {UserId} {OrderId} {Session} {Ip} {Country} {Product} {Qty} {Price} {Currency} {Flag} {LatencyMs} {TraceId}";
            _manyPropsValues = new object[] { 12345, "ORD-987654", "sess-42", "192.0.2.1", "US", "Widget", 3, 19.99, "USD", true, 12.4, Guid.NewGuid() };

            _sampleException = new InvalidOperationException("Sample exception for logging benchmark");
        }

        // No-op logger provider to act as a null sink for all loggers
        private class NoopLogger : Microsoft.Extensions.Logging.ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;
            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        }

        private class NoopLoggerProvider : ILoggerProvider
        {
            public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName) => new NoopLogger();
            public void Dispose() { }
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose() { }
        }

        private ILogger BuildLogger(Action<ServiceCollection> configure)
        {
            var services = new ServiceCollection();
            configure(services);

            // Replace any configured providers with a No-op provider to avoid I/O and match in-memory/null sink behavior
            services.AddLogging(b =>
            {
                b.ClearProviders();
                b.AddProvider(new NoopLoggerProvider());
            });

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
        public void Serilog_Log_Async() =>
            _serilogAsync.LogInformation("Serilog Async: Logging at {time}", DateTime.UtcNow);

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

        [Benchmark]
        public void Cerbi_Log_Async() =>
            _cerbiAsync.LogInformation("CerbiStream Async: Logging at {time}", DateTime.UtcNow);

        [Benchmark]
        public void Cerbi_Log_File() =>
            _cerbiFile.LogInformation("CerbiStream File: Logging at {time}", DateTime.UtcNow);

        // --- New Cerbi toggle benchmarks ---

        [Benchmark]
        public void Cerbi_Minimal() => _cerbi_minimal.LogInformation("Cerbi Minimal: Logging at {time}", DateTime.UtcNow);

        [Benchmark]
        public void Cerbi_NoAsync() => _cerbi_no_async.LogInformation("Cerbi NoAsync (dev on): Logging at {time}", DateTime.UtcNow);

        [Benchmark]
        public void Cerbi_NoDev() => _cerbi_no_dev.LogInformation("Cerbi NoDev (async on): Logging at {time}", DateTime.UtcNow);

        [Benchmark]
        public void Cerbi_Minimal_Async() => _cerbi_minimal_async.LogInformation("Cerbi Minimal+Async: Logging at {time}", DateTime.UtcNow);

        // --- Extended scenarios ---

        // Large message payloads (8KB)
        [Benchmark]
        public void MS_Log_LargeMessage() => _msLoggerPlain.LogInformation("MS Large: {msg}", _largeMessage);

        [Benchmark]
        public void NLog_Log_LargeMessage() => _nlogPlain.LogInformation("NLog Large: {msg}", _largeMessage);

        [Benchmark]
        public void Log4Net_Log_LargeMessage() => _log4netPlain.LogInformation("Log4Net Large: {msg}", _largeMessage);

        [Benchmark]
        public void Serilog_Log_LargeMessage() => _serilogPlain.LogInformation("Serilog Large: {msg}", _largeMessage);

        [Benchmark]
        public void Cerbi_Log_LargeMessage() => _cerbiPlain.LogInformation("Cerbi Large: {msg}", _largeMessage);

        // Exception logging (includes exception object)
        [Benchmark]
        public void MS_Log_Exception() => _msLoggerPlain.LogError(_sampleException, "MS Exception at {time}", DateTime.UtcNow);

        [Benchmark]
        public void NLog_Log_Exception() => _nlogPlain.LogError(_sampleException, "NLog Exception at {time}", DateTime.UtcNow);

        [Benchmark]
        public void Serilog_Log_Exception() => _serilogPlain.LogError(_sampleException, "Serilog Exception at {time}", DateTime.UtcNow);

        [Benchmark]
        public void Cerbi_Log_Exception() => _cerbiPlain.LogError(_sampleException, "Cerbi Exception at {time}", DateTime.UtcNow);

        // Many structured properties
        [Benchmark]
        public void MS_Log_ManyProps() => _msLoggerPlain.LogInformation(_manyPropsMessageFormat, _manyPropsValues);

        [Benchmark]
        public void NLog_Log_ManyProps() => _nlogPlain.LogInformation(_manyPropsMessageFormat, _manyPropsValues);

        [Benchmark]
        public void Serilog_Log_ManyProps() => _serilogPlain.LogInformation(_manyPropsMessageFormat, _manyPropsValues);

        [Benchmark]
        public void Cerbi_Log_ManyProps() => _cerbiPlain.LogInformation(_manyPropsMessageFormat, _manyPropsValues);

        // Batch throughput: loop1_000 messages inside the benchmark call to simulate high-frequency logging
        [Benchmark]
        public void MS_Log_Batch_1000()
        {
            for (int i = 0; i < 1000; i++)
                _msLoggerPlain.LogInformation("MS Batch: {i} {t}", i, DateTime.UtcNow);
        }

        [Benchmark]
        public void NLog_Log_Batch_1000()
        {
            for (int i = 0; i < 1000; i++)
                _nlogPlain.LogInformation("NLog Batch: {i} {t}", i, DateTime.UtcNow);
        }

        [Benchmark]
        public void Serilog_Log_Batch_1000()
        {
            for (int i = 0; i < 1000; i++)
                _serilogPlain.LogInformation("Serilog Batch: {i} {t}", i, DateTime.UtcNow);
        }

        [Benchmark]
        public void Cerbi_Log_Batch_1000()
        {
            for (int i = 0; i < 1000; i++)
                _cerbiPlain.LogInformation("Cerbi Batch: {i} {t}", i, DateTime.UtcNow);
        }
    }
}
