# Cerbi Benchmark Suite (In‑Depth)

A comprehensive, reproducible comparison of popular .NET logging stacks focused on throughput, allocation, encryption overhead, and governance cost.

- Target Framework: `.NET 9`
- Suite: `PopularLoggerBenchmarks`
- Host: Windows 11, Intel i9‑9900K, X64 RyuJIT x86‑64‑v3
- Tooling: BenchmarkDotNet v0.15.4, IterationCount=10, WarmupCount=3
- Artifacts: `Cerbi-Benchmark-Tests/BenchmarkDotNet.Artifacts/results`

This harness normalizes sinks by replacing providers with a no‑op sink. That isolates logger and pipeline overhead (no console/file/network I/O). Real sinks dominate cost in production and will shift absolute numbers.

---

## Executive summary

- Core overhead across MS, Serilog, NLog, log4net, and Cerbi is essentially identical for plain logs when I/O is removed (~60–64 ns/op).
- CerbiStream’s integrated Base64 encryption path stays near plain cost in this setup (~61 ns/op). Other stacks show ~4x cost for “encrypted formatting”. 
- Batching improves effective throughput; all stacks are within single‑digit percentages at 1,000‑message batches.
- Governance (PII detection/redaction) is orders of magnitude more expensive than logging itself; apply selectively.

---

## NEW: In‑Depth Comparative Analysis (Per Benchmark Category)

Below: What each benchmark measures, cross‑logger comparison, and practical meaning.

### 1. Plain Single Message (`*_Log_Plain`)
Purpose: Baseline per‑call overhead of emitting a simple structured log with a timestamp placeholder.
Comparison: All loggers cluster at ~62 ns. Differences (<3 ns) are statistically minor at this scale.
Meaning: Choice of logger does not materially impact raw call overhead for plain messages absent I/O.

### 2. Encrypted Single Message (`*_Log_Encrypted` vs `Cerbi_Log_Encrypted`)
Purpose: Cost of performing lightweight encryption/obfuscation prior to logging.
Comparison: MS/NLog/Serilog/log4net jump to ~250–270 ns (≈4x baseline). Cerbi remains ~61 ns (≈baseline).
Meaning: Cerbi integrates its light encryption in a way that avoids extra per‑call overhead here. Others incur formatting + transform cost. With real sinks, total latency will still be dominated by sink time but relative difference persists.

### 3. Heavy Encryption (`Cerbi_Log_Encrypted_Rsa`)
Purpose: Illustrative high‑cost algorithm (RSA) to show impact of heavy cryptography.
Comparison: Significantly higher than Base64 (check CSV for exact value; omitted here if truncated). Expected microseconds magnitude.
Meaning: Strong encryption per log is expensive; batch, cache keys, or encrypt at transport layer instead of per message if possible.

### 4. Async Variants (`Serilog_Log_Async`, `Cerbi_Log_Async`, Cerbi toggles)
Purpose: Overhead impact of enabling async pipeline structures (without real I/O).
Comparison: Async variants remain ~61–63 ns, effectively identical to plain.
Meaning: Async plumbing alone adds negligible overhead; real benefit appears once sinks perform blocking I/O.

### 5. Feature Toggles (Cerbi: `Minimal`, `NoAsync`, `NoDev`, `Minimal_Async`, `File`)
Purpose: Effect of enabling/disabling dev mode and async features.
Comparison: All toggles remain within ±3 ns of baseline.
Meaning: Internal conditional logic / lightweight configuration has negligible cost.

### 6. Large Message (`*_Log_LargeMessage` 8KB)
Purpose: Stress payload size without sink serialization cost.
Comparison: ~38–40 ns across all; smaller than plain because the benchmark uses fewer structured placeholders.
Meaning: Message length alone does not dominate when not allocating additional structures; real sinks will pay for encoding and transport.

### 7. Many Structured Properties (`*_Log_ManyProps` 12 props)
Purpose: Cost of template parsing and property binding.
Comparison: ~50–52 ns across all; no allocations (null sink avoids serialization).
Meaning: Structured property binding overhead is minimal; serialization & output dominate in real scenarios.

### 8. Exception Logging (`*_Log_Exception`)
Purpose: Include an Exception object (stack trace reference) in log call.
Comparison: ~60–63 ns; nearly same as plain since stack trace isn’t materialized by the null sink.
Meaning: Without output, holding exception reference is cheap; real sinks / enrichers may add cost when rendering.

### 9. Batching (`*_Log_Batch_10/100/1000`)
Purpose: Amortize overhead by looping multiple log calls inside a single benchmark invocation.
Comparison (1000 messages): 62.9K–68.0K ns total; difference <10% among libraries.
Meaning: Per‑message effective cost remains ≈60–68 ns. Batching primarily reduces scheduling / context overhead in real systems when combined with buffered sinks.

### 10. Governance – Runtime Regex (`Cerbi_Governance_*`)
Purpose: Simulate PII detection & redaction: simple string, structured dictionary, schema validation.
Comparison: Simple validate only ~492 ns (moderate); redaction operations ~42,000 ns (heavy).
Meaning: Regex scanning & string rebuilding dwarf raw logging cost. Apply only on sensitive flows; push detection to ingestion boundaries or design-time to minimize runtime redaction frequency.

### 11. Governance – JSON Driven (`Cerbi_GovernanceJson_*`)
Purpose: Same operations but patterns loaded from config for flexibility.
Comparison: Costs similar to corresponding runtime variants (see CSV). Loading overhead occurs once in setup.
Meaning: Externalizing rules does not add material steady-state cost; encourages iterative tuning without redeploy.

### 12. Design‑Time Governance (Roslyn Concept)
Purpose: Eliminate runtime scanning by preventing unsafe code at build/CI.
Comparison: Not benchmarked (0 runtime). Costs shift to developer feedback loop.
Meaning: Preferred for broad enforcement; reserve runtime redaction for uncertain or externally sourced data.

### Allocation Patterns
- Plain / structured / exception: ~56 B or 0 B; tiny, from framework formatting scaffolding.
- Encrypted (non‑Cerbi): 296–328 B due to string transformations.
- Governance heavy: KBs range (2.3–11.6 KB) from new redacted strings & dictionary copies.
- Batch: 88 KB for 1000 messages (≈88 B/message) in this synthetic scenario.

### Practical Priorities
1. Optimize sinks (async, batch, backpressure).
2. Centralize encryption; prefer integrated or transport-layer solutions.
3. Minimize heavy runtime governance; shift left with analyzers.
4. Batching + async provide scalability, not micro‑level savings in pure CPU.

---

## Fresh key results (Mean)

Plain single log cost (Mean ns): MS 62.23 | NLog 62.22 | Log4Net 62.75 | Serilog 64.45 | Cerbi 62.27
Encrypted single (Base64 or simulated): MS 266.94 | NLog 272.99 | Log4Net 249.56 | Serilog 256.75 | Cerbi 61.23
Batch 1000 total (ns): MS 62,914.50 | NLog 64,082.29 | Cerbi 67,184.55 | Serilog 68,084.14 (All allocate 88,000 B)
Large message (8KB) per op (ns): 38–40 across all
Many structured props (12) (ns): 50–52 across all (0 B allocated)
Exception logging (ns): ~60–63 across all
Governance (runtime regex): ValidateOnly 491.82 ns (2,392 B); Redact_Simple ~42,131 ns (11,632 B); Redact_Structured ~42,159 ns (9,296 B); Heavy ~42,418 ns (9,296 B)

---

## How to interpret

Developers
- Treat ~60 ns/op as the normalized baseline cost of a structured log invocation without I/O.
- Optimize sinks first: use async + batching; consider buffering and background shipping.
- Centralize encryption/redaction in the pipeline; avoid per‑call manual work.
- Governance regexes: keep sets minimal, compiled, and specific. Validate with production‑like payloads.
- Shift left with Roslyn analyzers (PII detection, required fields) to reduce runtime scanning and failures.

Non‑developers
- The choice of sink (console/file/cloud) dominates real‑world logging cost.
- Basic logging performance is similar across libraries.
- Security features (encryption, PII redaction) add cost—enable them only where needed.
- Sending logs in batches is more efficient for high‑rate systems.

---

## Methodology details

- `PopularLoggerBenchmarks` uses a null sink via a custom `NoopLoggerProvider` to remove I/O variability.
- BenchmarkDotNet job: `IterationCount=10`, `WarmupCount=3`, `MemoryDiagnoser` enabled.
- Encrypted (Base64) paths use CerbiStream’s integrated mode; others simulate encrypted formatting cost.
- RSA path encrypts payload bytes and logs base64—illustrative heavy mode.
- Governance runtime:
  - Regex patterns for email, credit card, SSN; structured traversal for dictionaries.
  - `governance.json` provides patterns, options, required fields, and action (`redact`/`drop`).
- Governance Roslyn (design‑time):
  - Use analyzers to prevent unsafe logging at build/CI time—no production runtime cost. This suite reports runtime costs only.

---

## Reproducing and slicing

- Run full suite: `dotnet run -c Release --project Cerbi-Benchmark-Tests/Cerbi-Benchmark-Tests.csproj`
- Open results: `Cerbi-Benchmark-Tests/BenchmarkDotNet.Artifacts/results` (`*.md`/`*.csv`/`*.html`)
- Tweak governance: edit `governance.json` and re‑run.
- Narrow scope: filter methods/categories with BenchmarkDotNet (optional; not configured in code for brevity).

---

## Feature comparison (capabilities snapshot)

- Structured logging: All
- Async sinks: All (via configuration/packages)
- Batching: All (via sinks/targets)
- Integrated “light” encryption: CerbiStream
- Heavy encryption demo (RSA): Provided in suite for Cerbi path
- Runtime governance redaction: Cerbi sample implementation (regex + JSON)
- Design‑time governance (Roslyn): Recommended for all stacks; separate analyzer tooling

Note: Capabilities depend on configuration and packages; consult each library’s docs for production setups.

---

## Limitations and future work

- Results exclude I/O and serialization cost—add real sinks to measure end‑to‑end pipelines.
- RSA sample is not tuned; production systems should reuse keys and leverage platform crypto.
- Add JSON serialization benchmarks (e.g., System.Text.Json) into the pipeline.
- Provide category filters and per‑scenario runners for quicker iteration.
- Include charts generated from CSV (scripts) for visual comparison.

---

## Appendix A: Benchmarks list

Core single‑message:
- `MS_Log_Plain`, `MS_Log_Encrypted`
- `Serilog_Log_Plain`, `Serilog_Log_Encrypted`, `Serilog_Log_Async`
- `NLog_Log_Plain`, `NLog_Log_Encrypted`
- `Log4Net_Log_Plain`, `Log4Net_Log_Encrypted`
- `Cerbi_Log_Plain`, `Cerbi_Log_Encrypted`, `Cerbi_Log_Encrypted_Rsa`, `Cerbi_Log_Async`, `Cerbi_Log_File`

Payload shapes:
- `*_Log_LargeMessage`, `*_Log_ManyProps`, `*_Log_Exception`

Batches:
- `*_Log_Batch_10`, `*_Log_Batch_100`, `*_Log_Batch_1000`

Cerbi toggles:
- `Cerbi_Minimal`, `Cerbi_NoAsync`, `Cerbi_NoDev`, `Cerbi_Minimal_Async`

Governance (runtime):
- `Cerbi_Governance_Redact_Simple`, `Cerbi_Governance_ValidateOnly`, `Cerbi_Governance_Redact_Structured`, `Cerbi_Governance_Heavy`

Governance (JSON):
- `Cerbi_GovernanceJson_Redact_Simple`, `Cerbi_GovernanceJson_ValidateOnly`, `Cerbi_GovernanceJson_Redact_Structured`, `Cerbi_GovernanceJson_Heavy`

---

## Appendix B: governance.json schema

- `piiRules[]`: `{ name, pattern, replacement, options[] }` (e.g., `Compiled`, `IgnoreCase`)
- `schema.requiredFields[]`: required keys for structured payloads
- `actions.onContainsPII`: behavior when PII is detected (`redact` or `drop`)

Adjust `governance.json` to evaluate rule complexity vs runtime overhead.
