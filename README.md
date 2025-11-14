# CerbiStream Benchmark & Evaluation Suite

Comprehensive performance and capability benchmarks comparing CerbiStream with established .NET logging libraries (Microsoft.Extensions.Logging, Serilog, NLog, log4net) under .NET 9. Designed both for engineers and decision makers evaluating CerbiStream.

## Why This Matters
Logging platforms underpin observability, security, compliance, and incident response. Selecting a platform involves balancing throughput, operational cost, feature depth (encryption/governance), and developer productivity. This suite shows where CerbiStream stands vs incumbents.

## Audience
- Developers: Understand micro overhead, feature trade-offs, and integration impacts.
- Architects/Ops: Gauge scaling characteristics, governance cost, and resource usage.
- Product/Non‑Technical Stakeholders: See value propositions in plain language—performance parity + enhanced encryption/governance efficiency.

## Scope of Benchmarks
Category | Purpose | CerbiStream Focus | Buyer Meaning
---------|---------|------------------|--------------
Plain Single | Baseline logger call cost | Parity with incumbents | No lock‑in penalty
Light Encryption | Obfuscate payload (Base64) | Integrated near-zero overhead | Lower infra cost vs custom encryption
Heavy Encryption | Stress test (RSA) | Illustrative worst-case | Shows impracticality of per-log heavy crypto
Async Variants | Non-blocking config | Minimal overhead | Scales without CPU tax
Feature Toggles | Dev/async modes | Negligible cost toggles | Safe to enable features
Large Payload | 8KB messages | Stable handling | Handles verbose diagnostic entries
Many Properties | Structured richness | Competitive binding | Enables deep context without cost
Exceptions | Error events | Stable baseline | Safe for frequent exception logging (with sinks tuned)
Batching | High-volume emission | Predictable scaling | High throughput potential
Governance Runtime | PII detect/redact | Clear cost boundaries | Transparency for compliance budgeting
Governance JSON | Config-driven rules | Dynamic tweakability | Faster adaptation to new policies
Design-Time Governance | Analyzer concept | Shift-left strategy | Reduced production overhead, proactive compliance

## Measurement Environment
- Framework: .NET 9 (RyuJIT x64)
- Host: Intel i9‑9900K, Windows 11
- Tool: BenchmarkDotNet (MemoryDiagnoser, IterationCount=10 WarmupCount=3)
- Sink Mode: No-op provider (removes console/file/network cost)
- Artifacts: `BenchmarkDotNet.Artifacts/results/*PopularLoggerBenchmarks*`

## Key Performance Findings (Latest Run)
Metric | CerbiStream | Competitors (Range) | Impact
------|-------------|---------------------|-------
Plain single (ns) | 62.27 | 62.22–64.45 | Parity
Encrypted single (ns) | 61.23 | 249.56–272.99 | 4x+ efficiency advantage
Batch 1000 total (ns) | 67,184.55 | 62,914.50–68,084.14 | Similar scaling
Heavy governance redaction (ns) | ~42,000 | N/A (sample only) | Feature cost transparency
Many properties (ns) | ~51 | ~50–52 | Parity
Large payload 8KB (ns) | ~39 | ~38–40 | Parity
Exception logging (ns) | ~62 | ~60–63 | Parity

## Interpreting Results (Developers)
Aspect | Meaning | Recommended Action
-------|--------|-------------------
Baseline Overhead | Parity indicates switching doesn’t hurt raw performance | Choose based on features not speculative speed gains
Encryption | Integrated light path avoids manual transforms & overhead | Use built-in encryption for sensitive fields
Heavy Crypto | Expensive per message | Prefer transport/batch encryption (TLS, AES) not RSA per log
Governance Cost | Large delta for redaction routines | Apply selectively; move detection to analyzers where possible
Batching | Linear scalability | Use buffered sinks; tune batch size (100–500) & flush interval
Async | Negligible overhead | Enable for I/O-bound sinks (file, network) to reduce tail latency
Structured Properties | Cheap binding | Leverage richer events; avoid unnecessary noise
Exceptions | Minimal baseline overhead | Log exceptions freely; optimize rendering/serialization pipeline

## Interpreting Results (Non‑Technical)
Concern | Plain Language Insight | Business Impact
--------|------------------------|---------------
Speed | CerbiStream as fast as top alternatives | No throughput penalty adopting CerbiStream
Encryption | CerbiStream can encrypt without slowing down | Lower compute cost & simpler compliance
Compliance (PII) | Detecting/removing sensitive data can be expensive | Budget only where needed; proactive tooling reduces runtime cost
Scalability | High-volume logging supported via batching/async | Predictable cost scaling
Reliability | Exceptions & rich context incur little overhead | Better incident visibility with minimal performance trade-off

## Value Proposition Summary
Dimension | CerbiStream Advantage
----------|---------------------
Performance | Baseline parity with mature stacks
Light Encryption | Near-zero overhead integrated path
Governance Flexibility | Runtime + JSON rules + analyzer strategy alignment
Operational Efficiency | Reduced per-call CPU/alloc when encrypting
Developer Productivity | Feature toggles without performance fear
Compliance Strategy | Transparent cost model + shift-left enablement

## Throughput Extrapolation (Theoretical)
Plain call ≈62 ns ⇒ ~16M calls/sec/core (ideal CPU-only). Real sinks drastically lower this; encryption in other libraries would cut theoretical capacity by ~4x on CPU-bound scenarios. CerbiStream preserves headroom for additional processing (serialization, shipping, metrics).

## Cost & Resource Perspective
Scenario | Extra CPU vs Plain | Allocation Impact | Business Meaning
--------|--------------------|-------------------|-----------------
Light encryption (others) | +~200 ns/op | +240–272 B/op | More cores/GC pressure under sensitive logging
Light encryption (Cerbi) | ≈0 ns | ≈0 B | Lower infra cost, simpler scaling
Heavy governance redaction | +~42,000 ns | +9–11 KB | Enable selectively; consider design-time blockers

## Governance Strategy Blueprint
Stage | Tooling | Runtime Impact | Benefit
-----|---------|---------------|--------
Design-Time | Roslyn analyzers (PII patterns, required fields) | None | Prevent leakage early
Runtime Light | ValidateOnly (~492 ns) | Low | Quick presence flagging
Runtime Full | Redaction (~42K ns) | High | Guaranteed output sanitization
Recommendation: Use analyzers broadly; restrict heavy redaction to untrusted or regulatory-bound ingestion paths.

## Integration Guidance
Use `AddCerbiStream` with desired options:
- Dev minimal + async: fast development iteration
- Encryption mode: enable where data classification requires
- Governance JSON: deploy new patterns without code redeploy

## Migration Playbook (From Existing Logger)
Step | Action | Effort | Risk
----|--------|-------|----
1 | Introduce CerbiStream as additional provider (dual logging) | Low | Low
2 | Validate encryption & governance in staging | Medium | Low
3 | Switch primary provider & reduce duplicates | Low | Low
4 | Add Roslyn analyzers in CI | Medium | Low
5 | Optimize batch + async sink configuration | Medium | Low

## Benchmark Method Categories (What They Demonstrate)
Type | Representative Methods | Core Question
-----|------------------------|--------------
Baseline | `*_Log_Plain` | Is logger overhead negligible? (Yes)
Encryption | `*_Log_Encrypted`, `Cerbi_Log_Encrypted` | Cost of lightweight encryption
Heavy Crypto | `Cerbi_Log_Encrypted_Rsa` | Upper bound penalty
Batch | `*_Log_Batch_10/100/1000` | Scaling vs single calls
Payload Size | `*_Log_LargeMessage` | Effect of large message
Rich Context | `*_Log_ManyProps` | Cost of multiple properties
Error | `*_Log_Exception` | Exception overhead baseline
Governance Runtime | `Cerbi_Governance_*` | Cost of PII detection/redaction
Governance JSON | `Cerbi_GovernanceJson_*` | Config-driven rule flexibility
Feature Toggles | `Cerbi_Minimal/NoAsync/NoDev/...` | Impact of toggling behaviors

## Reproducing
```
dotnet run -c Release --project Cerbi-Benchmark-Tests/Cerbi-Benchmark-Tests.csproj
```
Artifacts: `BenchmarkDotNet.Artifacts/results`
Governance rules: `governance.json`

## Extending the Suite
Add real sinks (console/file/JSON/network), serialization benchmarks (System.Text.Json vs Newtonsoft.Json), layered enrichment comparisons, and CI automation to regenerate metrics.

## Limitations
- No I/O sink cost included (intentional isolation)
- RSA heavy encryption is illustrative only (not optimized)
- Figures are hardware-specific; rerun on target infra

## Appendix A: Full Benchmark List
(Existing list retained)

## Appendix B: governance.json Schema
- `piiRules[]`: { name, pattern, replacement, options[] }
- `schema.requiredFields[]`: required structured keys
- `actions.onContainsPII`: `redact` | `drop`

## License / Attribution
Benchmark definitions for comparative evaluation; results for decision support. Re-run for confirmation.
