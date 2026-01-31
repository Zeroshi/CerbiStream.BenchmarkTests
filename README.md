# CerbiStream Benchmark Suite

High-assurance .NET logging with governance, redaction, and encryption built-in.

CerbiStream 1.1.88 vs Serilog, NLog, log4net, MS Logging on .NET 9

## Key Results

Plain logging: All loggers ~62-68 ns (parity)
Encrypted: CerbiStream 63.92 ns vs competitors 250+ ns (4x faster)
PII redaction: CerbiStream 90 ns vs DIY 750+ ns (8x faster)
Design-time governance: Zero runtime cost

See Cerbi-Benchmark-Tests/README.md for full analysis.

Visit cerbi.io for documentation.
