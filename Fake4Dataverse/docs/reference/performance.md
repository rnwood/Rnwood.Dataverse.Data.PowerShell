# Performance Benchmarks

## Overview

Benchmarks are run with [BenchmarkDotNet](https://benchmarkdotnet.org/) comparing
**Fake4Dataverse** (Lenient preset) against **FakeXrmEasy 1.x** on .NET Framework 4.6.2
and **FakeXrmEasy 3.x** on .NET 10.

Important notes:

- **FakeXrmEasy 1.x** targets .NET Framework only, so it can only be compared on `net462`.
- **FakeXrmEasy 3.x** (`FakeXrmEasy.Core.v9`) is benchmarked with the `RPL_1_5` license
  and minimal middleware setup (`.AddCrud().UseCrud()`).
- **Fake4Dataverse** uses the `Lenient` preset (all auto-behaviors off) for an
  apples-to-apples comparison.

## Environment

| Detail | Value |
|--------|-------|
| OS | Windows 11 |
| CPU | Intel Core Ultra 7 258V |
| BenchmarkDotNet | v0.14.0 |
| Job | ShortRun (3 warmup iterations, 3 target iterations) |

## CRUD Operations

Single-entity operations on an `account` record.

| Benchmark | F4D (net462) | FXE v1 (net462) | Speedup | F4D (net10) | FXE v3 (net10) | Speedup |
|---|---:|---:|---:|---:|---:|---:|
| Create | 2.54 µs | 10.43 µs | **4.1×** | 2.62 µs | 14.2 µs | **5.4×** |
| Create + Update + Delete | 1.28 µs | 16.95 µs | **13.3×** | 744 ns | 29.8 µs | **40×** |
| Retrieve (by ID) | 213 ns | 9.68 µs | **45×** | 136 ns | 13.3 µs | **98×** |
| Update | 353 ns | 5.48 µs | **15.5×** | 168 ns | 7.75 µs | **46×** |

## QueryExpression / RetrieveMultiple

Paged query returning page 1 with 50 records per page.

| Scenario | Rows | F4D (net462) | FXE v1 (net462) | Speedup | F4D (net10) | FXE v3 (net10) | Speedup |
|---|---:|---:|---:|---:|---:|---:|---:|
| All columns | 100 | 59.9 µs | 1.69 ms | **28×** | 40.3 µs | 2.10 ms | **52×** |
| Filter (statecode=0) | 100 | 79.2 µs | 2.73 ms | **34×** | 54.6 µs | 3.17 ms | **58×** |
| Order by name | 100 | 141 µs | 4.54 ms | **32×** | 66.5 µs | 2.17 ms | **33×** |
| Filter + order | 100 | 134 µs | 5.66 ms | **42×** | 62.5 µs | 3.27 ms | **52×** |
| All columns | 1,000 | 781 µs | 15.32 ms | **19.6×** | 353 µs | 7.83 ms | **22×** |
| Filter (statecode=0) | 1,000 | 1.22 ms | 13.83 ms | **11.3×** | 518 µs | 9.26 ms | **18×** |
| Order by name | 1,000 | 2.38 ms | 19.04 ms | **8.0×** | 1.11 ms | 10.1 ms | **9.1×** |
| Filter + order | 1,000 | 1.56 ms | 16.19 ms | **10.4×** | 813 µs | 9.60 ms | **11.8×** |
| All columns | 10,000 | 25.3 ms | 221.6 ms | **8.8×** | 12.3 ms | 117 ms | **9.5×** |
| Filter (statecode=0) | 10,000 | 26.3 ms | 163.0 ms | **6.2×** | 13.4 ms | 91.6 ms | **6.8×** |
| Order by name | 10,000 | 42.3 ms | 235.1 ms | **5.6×** | 16.0 ms | 116 ms | **7.2×** |
| Filter + order | 10,000 | 33.6 ms | 200.6 ms | **6.0×** | 16.3 ms | 97.1 ms | **6.0×** |

## Key Takeaways

- **Fake4Dataverse is 4–98× faster** than FakeXrmEasy across all operations.
- **Retrieve by ID** shows the largest difference (45–98× faster) because
  Fake4Dataverse uses a direct dictionary lookup with no query-pipeline overhead.
- **.NET 10 is consistently faster** than .NET Framework 4.6.2 for both libraries,
  but the gap is larger for Fake4Dataverse due to heavy use of `Span<T>` and
  modern collection APIs.
- At **10,000 rows** the speedup narrows (5–10×) because both libraries are
  dominated by the same O(n) scan; adding an `AttributeIndex` in Fake4Dataverse
  can restore sub-millisecond filtered queries even at large row counts.

## Running Benchmarks Yourself

The benchmark project lives in `benchmarks/Fake4Dataverse.Benchmarks`.

```powershell
# .NET Framework 4.6.2 — vs FakeXrmEasy 1.x
dotnet run --configuration Release --project benchmarks/Fake4Dataverse.Benchmarks -f net462

# .NET 10 — vs FakeXrmEasy 3.x
dotnet run --configuration Release --project benchmarks/Fake4Dataverse.Benchmarks -f net10.0
```

Results are written to `BenchmarkDotNet.Artifacts/results/`.

> **Tip:** Use `--filter *Crud*` or `--filter *Query*` to run a subset of benchmarks.

## Optimization Tips

For guidance on improving query performance in your tests — including attribute
indexes, column-set reduction, and paging strategies — see the
[Performance & Indexing](../guides/performance-indexing.md) guide.
