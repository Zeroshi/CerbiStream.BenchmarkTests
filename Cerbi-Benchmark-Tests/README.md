# Cerbi Benchmark Suite

A simple, reproducible comparison of popular .NET loggers using BenchmarkDotNet.

- Project: .NET9
- Benchmarks: `PopularLoggerBenchmarks`
- Artifacts: `Cerbi-Benchmark-Tests/BenchmarkDotNet.Artifacts/results`

---

## TL;DR (Non‑technical summary)

- With logging effectively disabled (no‑op sink), all loggers are equally fast; differences are tiny (nanoseconds).
- Real‑world speed is dominated by the output sink (console/file/network). Use async/batching.
- CerbiStream sits alongside veteran loggers (Serilog, NLog, log4net) on core overhead. With integrated encryption, Cerbi shows very low overhead in this benchmark setup.

---

## Latest run (CerbiStream1.1.70, .NET9, normalized/no‑op sink)

Source: `BenchmarkDotNet.Artifacts/results/CerbiBenchmark.PopularLoggerBenchmarks-report.csv`

Lower is faster. Mean shown; allocations are per operation.

- Single small message (Mean):
 - Cerbi_Log_Plain:62.27 ns
 - NLog_Log_Plain:62.22 ns
 - Log4Net_Log_Plain:62.75 ns
 - Serilog_Log_Plain:64.45 ns
 - MS_Log_Plain:62.23 ns

- Encrypted message (Mean):
 - Cerbi_Log_Encrypted:61.23 ns
 - Log4Net_Log_Encrypted:249.56 ns
 - Serilog_Log_Encrypted:256.75 ns
 - MS_Log_Encrypted:266.94 ns
 - NLog_Log_Encrypted:272.99 ns

- Batch throughput (1,000 logs total time):
 - MS_Log_Batch_1000:62,914.50 ns
 - NLog_Log_Batch_1000:64,082.29 ns
 - Cerbi_Log_Batch_1000:67,184.55 ns
 - Serilog_Log_Batch_1000:68,084.14 ns

- Many structured properties (12 props, Mean): ~50–52 ns across all loggers
- Large message placeholder (Mean): ~38–40 ns across all loggers
- Exception logging (Mean): ~60–63 ns across all loggers

Governance (Cerbi‑specific, new in suite):
- Cerbi_Governance_ValidateOnly:491.82 ns, ~2.3 KB allocated
- Cerbi_Governance_Redact_Structured: ~42,158 ns, ~9.1 KB allocated
- Cerbi_Governance_Redact_Simple: ~42,131 ns, ~11.4 KB allocated
- Cerbi_Governance_Heavy: ~42,418 ns, ~9.1 KB allocated

Notes:
- This run uses a no‑op sink to isolate logger overhead. Real sinks (console/file/network) will change results.
- Cerbi’s encrypted path is very fast here because encryption cost is integrated and the sink is no‑op.

---

## What these results mean (plain English)

- Logger call overhead is tiny and comparable across all libraries in a normalized setup.
- Sinks dominate in production. Configure async/batching for throughput regardless of logger.
- CerbiStream is a comparable choice to veteran loggers; with the same sink setup you should expect similar throughput. Its integrated encryption can reduce extra work in the call site.

---

## Reproducing the run

- Run: `dotnet run -c Release`
- Results: `Cerbi-Benchmark-Tests/BenchmarkDotNet.Artifacts/results`
- Open: `CerbiBenchmark.PopularLoggerBenchmarks-report-github.md` or `.csv/.html`

---

## Notes on images/charts

Older screenshots/charts have been removed/omitted. Use the generated HTML/CSV/Markdown in the artifacts folder for up‑to‑date visuals and tables.
