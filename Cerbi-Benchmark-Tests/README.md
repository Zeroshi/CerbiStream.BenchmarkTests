# Cerbi Benchmark Suite

A simple, reproducible comparison of popular .NET loggers using BenchmarkDotNet.

- Project: .NET8
- Benchmarks: `PopularLoggerBenchmarks`
- Artifacts: `Cerbi-Benchmark-Tests/BenchmarkDotNet.Artifacts/results`

---

## TL;DR (Non‑technical summary)

- In a like‑for‑like test with logging effectively disabled (no‑op/"null" sink), all loggers are equally fast. Differences are tiny and in nanoseconds.
- With encryption enabled in this benchmark, `CerbiStream` looks much faster because the encryption is integrated into its pipeline, while other loggers were given pre‑encrypted strings. In real apps, encrypting inside the logger vs. before logging changes where the cost is paid.
- For high‑throughput scenarios, the output sink (console/file/network) dominates performance. Use async/batching regardless of logger choice.
- Bottom line: `CerbiStream` sits alongside veteran loggers (Serilog, NLog, log4net) on core overhead. Real‑world speed mostly depends on sink configuration, not the logger brand.

---

## Latest results (normalized/no‑op sink)

This run replaces all providers with a no‑op sink to measure only each logger’s call overhead (no console/file I/O). Lower is faster.

Source: `BenchmarkDotNet.Artifacts/results/CerbiBenchmark.PopularLoggerBenchmarks-report-github.md`

- Single small message (Mean):
 - `Log4Net_Log_Plain`:84.82 ns
 - `Serilog_Log_Plain`:88.39 ns
 - `Cerbi_Log_Plain`:89.39 ns
 - `NLog_Log_Plain`:92.74 ns
 - `MS_Log_Plain`:94.95 ns

- Encrypted message (Mean):
 - `Cerbi_Log_Encrypted`:88.12 ns
 - `Log4Net_Log_Encrypted`:368.83 ns
 - `Serilog_Log_Encrypted`:376.31 ns
 - `MS_Log_Encrypted`:391.32 ns
 - `NLog_Log_Encrypted`:401.14 ns

- Batch throughput (1,000 logs total time):
 - `Cerbi_Log_Batch_1000`:87,248.38 ns
 - `NLog_Log_Batch_1000`:87,440.41 ns
 - `Serilog_Log_Batch_1000`:90,590.68 ns
 - `MS_Log_Batch_1000`:91,635.39 ns

- Many structured properties (Mean,12 props): ~69–71 ns across all loggers
- Large message (8 KB placeholder, Mean): ~54–56 ns across all loggers

Why are many results so similar? Because with a no‑op sink and `IsEnabled=false`, most work is skipped. This isolates the core overhead of calling the logger, which is very small and comparable across libraries.

---

## What these results mean (plain English)

- Logger overhead is tiny: All tested loggers add roughly the same, very small CPU cost when logging is disabled or filtered out.
- Sinks dominate in production: Writing to console, files, or the network is usually the bottleneck. Configure async/batching to keep the app fast.
- Encryption numbers need context: In this suite, non‑Cerbi loggers were fed pre‑encrypted text, while `CerbiStream` encrypted internally. That’s why Cerbi’s encrypted path appears much faster here. If others encrypted internally too, the results would be closer.
- Throughput: In the batch test, `CerbiStream` and `NLog` were marginally fastest. Differences are small and unlikely to matter without I/O.

---

## How `CerbiStream` compares to veteran loggers

- Core overhead: On par with Serilog, NLog, and log4net in this no‑op configuration.
- Encryption integration: Appears very efficient in this benchmark because cost is inside the pipeline; others were measured with pre‑encrypted inputs.
- Real‑world usage: Expect similar performance to veteran loggers when configured with the same sinks. The sink choice (and async/batching) will determine overall speed.

---

## Reproducing the run

- Run: `dotnet run -c Release`
- Results: `Cerbi-Benchmark-Tests/BenchmarkDotNet.Artifacts/results`
- Open: `CerbiBenchmark.PopularLoggerBenchmarks-report-github.md` (and the HTML/CSV alongside it)

---

## Tips for real apps

- Prefer async/batched sinks over synchronous console/file writes.
- Keep hot‑path logs lean (fewer structured properties, small messages).
- Avoid pushing large payloads through logging; log identifiers and store blobs separately.
- Measure in your environment; I/O and filtering change results more than library choice.
