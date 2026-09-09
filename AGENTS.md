# AGENTS.md

Instructions for AI coding agents working in this repository.

## Overview

Succinct LINQ is a set of Roslyn analyzers for C# that suggest more succinct LINQ
expressions. The solution contains:

- `SuccinctLinq.Analyzers` — the analyzer package (targets `netstandard2.0`)
- `SuccinctLinq.Analyzers.Test` — xUnit v3 tests (targets `net10.0`, Microsoft.Testing.Platform)

Requires the .NET 10 SDK.

## Building

```sh
dotnet build
```

## Running tests

Tests use xUnit v3 on Microsoft.Testing.Platform (configured in `global.json`), so
`dotnet test` runs them directly:

```sh
dotnet test
```

You can also run the test host directly:

```sh
dotnet run --project SuccinctLinq.Analyzers.Test
```

### Filtering tests

Arguments after `--` are passed to the test host. Supported filters (see
`--help` for the full list):

```sh
# Run a single test method
dotnet test -- --filter-method "SuccinctLinq.Analyzers.Test.Tests.RedundantDistinctAnalyzerTests.RedundantDistinct_DistinctThenToHashSet_ReportWarning"

# Run all tests in a class (wildcards supported)
dotnet test -- --filter-class "SuccinctLinq.Analyzers.Test.Tests.RedundantDistinctAnalyzerTests"

# Run all tests in a namespace
dotnet test -- --filter-namespace "SuccinctLinq.Analyzers.Test.Tests"

# VSTest-style filter syntax
dotnet test -- --filter "FullyQualifiedName~RedundantDistinct"
```

## Adding a new analyzer rule

1. Add a `sealed` class named after the rule (e.g. `RedundantDistinctAnalyzer`) in
   `SuccinctLinq.Analyzers/Rules/`, deriving `DiagnosticAnalyzer` and decorated
   with `[DiagnosticAnalyzer(LanguageNames.CSharp)]`.
2. Assign the next available `SLQ`-prefixed rule ID (existing: SLQ1001, SLQ1002,
   SLQ1101).
3. Register the rule in `SuccinctLinq.Analyzers/AnalyzerReleases.Unshipped.md`.
4. Reuse or extend the shared helpers in `SuccinctLinq.Analyzers/Extensions/`.
5. Add a test class in `SuccinctLinq.Analyzers.Test/Tests/` (see below).

## Test conventions

- One `*Tests` class per analyzer in the namespace `SuccinctLinq.Analyzers.Test.Tests`.
- Test methods are named `<Rule>_<Scenario>_<ExpectedResult>` (e.g.
  `RedundantDistinct_DistinctThenToHashSet_ReportWarning`) and return `Task`,
  passed `TestContext.Current.CancellationToken`.
- Build the test with `AnalyzerTest.CreateContext<YourAnalyzer>()`, set
  `context.TestCode` as a raw string literal, and mark expected diagnostics with
  `{|SLQ1001:Distinct()|}` markup.

## Constraints

- The analyzer project targets `netstandard2.0` (the Polyfill package supplies
  missing APIs). Keep analyzer code allocation-free and thread-safe;
  `EnforceExtendedAnalyzerRules` is enabled.
- `BannedSymbols.txt` bans `Enumerable.First`, `Nullable.Value`, and
  `ISymbol.ToDisplayString(SymbolDisplayFormat)` — check type names and
  namespaces instead of building display strings.
- Code style is enforced at build time by `.editorconfig` and a large set of
  analyzers (`AnalysisLevel` 10-all, `EnforceCodeStyleInBuild`). Source files
  use CRLF line endings.

## CI

GitHub Actions (`.github/workflows/dotnet.yml`) runs on Windows with:
`dotnet restore && dotnet build --no-restore && dotnet test --no-build
--report-trx --coverage`. Use the same commands locally to match CI.
