# CerbiStream Benchmark & Evaluation Suite

Comprehensive performance and capability benchmarks comparing **CerbiStream** with established .NET logging libraries:

- **Microsoft.Extensions.Logging (MEL)**
- **Serilog**
- **NLog**
- **log4net**

All benchmarks are targeted at **.NET 9** and designed for both **engineers** and **decision makers** evaluating CerbiStream against incumbent logging stacks.

---

## 💡 Why This Matters

Logging underpins:

- Observability & SRE
- Security & compliance
- Incident response & forensics
- Cost management (storage + compute)

Choosing a logging platform isn’t just “which one writes to console” — it’s a trade-off between:

- Throughput & CPU/GC overhead
- Feature depth (encryption, governance, PII control)
- Complexity of integration
- Ongoing operational cost

This suite shows **where CerbiStream stands vs the incumbent loggers**, with a focus on:

- **Performance parity** in the baseline hot path  
- **Differentiated efficiency** when you turn on encryption & governance

---

## 🎯 Audience

**Developers**

- Understand per-call overhead.
- See how CerbiStream behaves with encryption, governance, batching, async.
- Decide whether governance/encryption is safe “always on”.

**Architects / SRE / Ops**

- Gauge scaling characteristics under load.
- Understand the true cost of encryption + PII redaction.
- See how CerbiStream compares to MEL, Serilog, NLog, log4net when tuned for high throughput.

**Product / Non-Technical Stakeholders**

- Plain-language summaries of:
  - Performance parity with mature loggers.
  - Where CerbiStream saves CPU/infra cost.
  - How built-in governance reduces risk vs “just log everything”.

---

## 📊 Scope of Benchmarks

| Category          | Purpose                          | CerbiStream Focus                      | Buyer Meaning                               |
|-------------------|----------------------------------|----------------------------------------|---------------------------------------------|
| Plain Single      | Baseline logger call cost        | Parity with incumbents                 | No lock-in performance penalty              |
| Light Encryption  | Obfuscate payload (Base64)       | Integrated near-zero overhead          | Lower infra cost vs DIY encryption          |
| Heavy Encryption  | Stress test (RSA)                | Illustrative worst-case                | Shows impracticality of per-log heavy crypto |
| Async Variants    | Non-blocking config              | Minimal overhead                        | Scales without extra CPU tax                |
| Feature Toggles   | Dev/async/governance modes       | Negligible toggle cost                 | Safe to enable features                     |
| Large Payload     | 8KB messages                     | Stable handling                        | Handles verbose diagnostics cleanly         |
| Many Properties   | Structured richness              | Competitive binding                    | Deep context without penalty                |
| Exceptions        | Error events                     | Stable baseline                        | Safe to log exceptions frequently           |
| Batching          | High-volume emission             | Predictable scaling                    | High throughput potential                   |
| Governance Runtime| PII detect/redact                | Clear cost boundaries                  | Transparent compliance budgeting            |
| Governance JSON   | Config-driven rules              | Dynamic tweakability                   | Faster adaptation to new policies           |
| Design-Time Gov   | Analyzer concept                 | Shift-left governance strategy         | Reduced production risk + overhead          |

---

## 🧪 Measurement Environment

- **Framework:** .NET 9 (RyuJIT x64)
- **Host:** Intel i9-9900K, Windows 11
- **Tooling:** BenchmarkDotNet  
  - `MemoryDiagnoser`
  - `IterationCount = 10`, `WarmupCount = 3`
- **Sink mode:** **No-op provider** (removes console/file/network cost to isolate logger overhead)
- **Artifacts:**  
  `BenchmarkDotNet.Artifacts/results/*PopularLoggerBenchmarks*`

> Real-world scenarios (Seq, Loki, ELK/OpenSearch, Graylog, OTLP/OTEL Collector, etc.) will be I/O-bound. These benchmarks isolate the **logger hot path**, not the sinks.

---

## 📌 Key Performance Findings (Latest Sample Run)

| Metric                         | CerbiStream          | Top Loggers (Range)       | Impact                                      |
|--------------------------------|----------------------|---------------------------|---------------------------------------------|
| Plain single (ns)              | 62.27               | 62.22–64.45               | Parity                                      |
| Encrypted single (ns)          | 61.23               | 249.56–272.99             | **4x+ efficiency advantage**                |
| Batch 1000 total (ns)          | 67,184.55           | 62,914.50–68,084.14       | Similar scaling                             |
| Heavy governance redaction (ns)| ~42,000             | N/A (Cerbi-only scenario) | Clear visibility into worst-case cost       |
| Many properties (ns)           | ~51                 | ~50–52                    | Parity                                      |
| Large payload 8KB (ns)         | ~39                 | ~38–40                    | Parity                                      |
| Exception logging (ns)         | ~62                 | ~60–63                    | Parity                                      |

Bottom line:

- **Baseline:** CerbiStream is as fast as MEL/Serilog/NLog/log4net in the hot path.
- **Encryption:** CerbiStream’s integrated light encryption path is dramatically cheaper than “roll your own” transforms.
- **Governance:** Heavy governance modes are visibly more expensive — on purpose — so you can **budget and apply them intelligently**.

---

## 👨‍💻 Interpreting Results (Developers)

| Aspect              | Meaning                                           | Recommended Action                                   |
|---------------------|---------------------------------------------------|------------------------------------------------------|
| Baseline Overhead   | Parity with incumbents                            | Choose based on features, not speculative micro-gains |
| Encryption          | Built-in light path is effectively free           | Use Cerbi’s integrated encryption for sensitive fields |
| Heavy Crypto        | Expensive per message                             | Use TLS/AES at transport/batch; avoid RSA per log    |
| Governance Cost     | Redaction routines are intentionally heavier      | Apply full redaction selectively; lean on analyzers  |
| Batching            | Linear scaling across logger families             | Use buffered sinks; tune batch size (100–500) + flush |
| Async               | Negligible overhead in logger layer               | Enable async for I/O-bound sinks (file/network/OTEL) |
| Structured Props    | Cheap property binding                            | Use rich context; avoid spammy, meaningless fields   |
| Exceptions          | Minimal baseline impact                           | Log exceptions freely; optimize formatting as needed |

---

## 🧾 Interpreting Results (Non-Technical)

| Concern     | Plain-Language Insight                                             | Business Impact                            |
|------------|----------------------------------------------------------------------|-------------------------------------------|
| Speed      | CerbiStream is as fast as the most popular .NET loggers            | No performance penalty for adopting it    |
| Encryption | CerbiStream encrypts sensitive data without dragging performance    | Lower compute cost, easier compliance     |
| Compliance | Detecting/removing sensitive data is more expensive than plain logs | Use it where it matters; don’t overuse    |
| Scalability| High-volume logging works well with batching & async                | Predictable scaling as traffic grows      |
| Reliability| Exceptions & rich context are cheap to log                          | Better visibility during incidents        |

---

## 📌 Value Proposition Summary

| Dimension             | CerbiStream Advantage                                                    |
|-----------------------|---------------------------------------------------------------------------|
| Performance           | Baseline parity with MEL/Serilog/NLog/log4net                           |
| Light Encryption      | Near-zero overhead integrated path                                       |
| Governance Flexibility| Runtime validation + JSON rules + design-time analyzers                  |
| Operational Efficiency| Less CPU/alloc cost when encrypting or tagging governance metadata       |
| Developer Productivity| Feature toggles (async, encryption, governance) without perf anxiety     |
| Compliance Strategy   | Transparent cost model, shift-left analyzers, runtime safety nets        |

---

## 📈 Throughput Extrapolation (Theoretical)

- **Plain call:** ~62 ns ⇒ ~16M calls/second/core in ideal CPU-only scenarios.
- Real sinks (file, console, OTLP, Kafka, HTTP, etc.) drastically reduce this, because I/O dominates.
- Competing libraries performing equivalent encryption per log would cut theoretical capacity by **~4x** in CPU-bound scenarios.
- CerbiStream preserves headroom so you can still:
  - Serialize to JSON
  - Enrich metrics
  - Ship to OTEL Collector, Seq, Loki, ELK/OpenSearch, Graylog, etc.

---

## 💰 Cost & Resource Perspective

| Scenario                   | Extra CPU vs Plain | Allocation Impact  | Business Meaning                                  |
|----------------------------|--------------------|--------------------|---------------------------------------------------|
| Light encryption (others)  | +~200 ns/op        | +240–272 B/op      | More cores, more GC pressure under sensitive logs |
| Light encryption (Cerbi)   | ≈0 ns              | ≈0 B               | Lower infra cost, simpler scaling                 |
| Heavy governance redaction | +~42,000 ns        | +9–11 KB           | Use only where absolutely required                |

---

## 🛡 Governance Strategy Blueprint

| Stage         | Tooling                                   | Runtime Impact | Benefit                                  |
|---------------|-------------------------------------------|----------------|------------------------------------------|
| Design-Time   | Roslyn analyzers (PII patterns, fields)   | None           | Prevent leakage before merge/deploy      |
| Runtime Light | `ValidateOnly` (~492 ns)                  | Low            | Cheap presence/shape validation          |
| Runtime Full  | Full redaction (~42K ns)                  | High           | Guaranteed sanitization of sensitive data|

**Recommended pattern:**

- Use **analyzers broadly** across all governed services.
- Use **runtime light validation** widely for safety.
- Use **heavy redaction** in:
  - ingestion edges,
  - regulatory-bound flows,
  - or untrusted sources where you can’t trust upstream sanitization.

---

## 🔗 Integration Guidance (CerbiStream)

Typical wiring via `AddCerbiStream`:

- **Dev / minimal / async mode:**
  - Fast feedback, low ceremony.
  - Great for local, test, integration environments.
- **Encryption mode:**
  - Enable wherever data classification requires protection.
  - Configure via Cerbi options or JSON governance profile.
- **Governance JSON:**
  - Deploy new rules without redeploying code.
  - Aligns with CerbiShield + Governance Analyzer.

---

## 🔁 Migration Playbook (From Existing Logger)

| Step | Action                                             | Effort | Risk |
|------|----------------------------------------------------|--------|------|
| 1    | Introduce CerbiStream as **additional** provider (dual logging) | Low    | Low  |
| 2    | Validate encryption & governance in staging        | Medium | Low  |
| 3    | Switch primary provider, phase out duplicate logs  | Low    | Low  |
| 4    | Add Roslyn analyzers into CI                       | Medium | Low  |
| 5    | Tune batch sizes + async sinks per environment     | Medium | Low  |

---

## 🧪 Benchmark Method Categories

| Type              | Representative Methods              | Core Question                                  |
|-------------------|-------------------------------------|-----------------------------------------------|
| Baseline          | `*_Log_Plain`                       | Is logger overhead negligible? (Yes)          |
| Encryption        | `*_Log_Encrypted`, `Cerbi_Log_Encrypted` | Cost of lightweight encryption           |
| Heavy Crypto      | `Cerbi_Log_Encrypted_Rsa`           | Upper bound penalty for overkill crypto       |
| Batch             | `*_Log_Batch_10/100/1000`          | Scaling vs single calls                       |
| Payload Size      | `*_Log_LargeMessage`                | Impact of large messages                      |
| Rich Context      | `*_Log_ManyProps`                   | Cost of multiple structured properties        |
| Error             | `*_Log_Exception`                   | Exception overhead baseline                   |
| Governance Runtime| `Cerbi_Governance_*`                | Cost of PII detection / redaction             |
| Governance JSON   | `Cerbi_GovernanceJson_*`            | Flexibility of JSON-configured rules          |
| Feature Toggles   | `Cerbi_Minimal/NoAsync/NoDev/...`   | Impact of toggle combinations                 |

---

## 🔁 Reproducing the Benchmarks

```bash
dotnet run -c Release --project Cerbi-Benchmark-Tests/Cerbi-Benchmark-Tests.csproj
````

* Artifacts: `BenchmarkDotNet.Artifacts/results`
* Governance rules: `governance.json`

Run on your target hardware to validate numbers in your environment.

---

## ➕ Extending the Suite

Useful additions:

* Real sinks: console, file, JSON, OTLP, TCP/HTTP
* Serialization comparisons:

  * `System.Text.Json` vs `Newtonsoft.Json`
* Enrichment benchmarks:

  * Contextual enrichers vs lightweight tags
* CI integration:

  * Nightly or per-branch benchmark runs
  * Trend tracking over time

---

## ⚠ Limitations

* No I/O sink cost included (by design — isolates logger overhead).
* RSA heavy encryption is **illustrative**, not optimized for real production use.
* Figures are **hardware-specific**. Always re-run on your own hardware / cloud.

---

## Appendix A: Full Benchmark List

> Existing list retained in the repo (see `Benchmarks/*` and `BenchmarkDotNet.Artifacts/results/*`).

---

## Appendix B: `governance.json` Schema (Sample)

Minimal example:

```json
{
  "piiRules": [
    {
      "name": "SSN",
      "pattern": "\\b\\d{3}-\\d{2}-\\d{4}\\b",
      "replacement": "***-**-****",
      "options": [ "IgnoreCase" ]
    }
  ],
  "schema": {
    "requiredFields": [ "userId", "correlationId" ]
  },
  "actions": {
    "onContainsPII": "redact"  // or "drop"
  }
}
```

Actual schema is defined in Cerbi governance core and used across:

* CerbiStream
* Cerbi Governance Analyzer
* Cerbi Runtime Governance
* CerbiShield (governance dashboard)

---

## License / Attribution

Benchmark definitions are provided for comparative evaluation and decision support.
Always re-run on your infrastructure to confirm results.

MIT License – see repository license file.