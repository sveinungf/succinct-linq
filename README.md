# Succinct LINQ

Succinct LINQ is a set of Roslyn analyzers for C# that flag redundant and verbose LINQ expressions and suggest more concise equivalents.

Add the package to any C# project and the rules run as part of the normal build. Every rule is enabled by default and reports as a warning.


## Background

LINQ code can sometimes accumulate redundant work: a `Distinct()` call left behind after a later refactor to `ToHashSet()`, 
a `Select(x => x)` that no longer maps anything, or a `ToDictionary(x => x.Length, x => x)` where the second lambda just returns the element it was given. 
None of these are wrong, but they make code harder to read and can cost extra allocations at run time.

Succinct LINQ exists to catch exactly these patterns, in two categories:

- **Redundancy** — calls that do nothing because a later call in the chain already provides the same behavior, or selectors that merely return their input.
- **Simplification** — calls that can be replaced by a shorter, more idiomatic API, such as `OrderBy(x => x)` with `Order()`.


## How to install

Succinct LINQ is available as a [package on nuget.org](https://www.nuget.org/packages/SuccinctLinq.Analyzers).

### .NET CLI
```
dotnet add package SuccinctLinq.Analyzers
```
### Package Manager
```
Install-Package SuccinctLinq.Analyzers
```


## Rules

| Rule ID | Category | Severity | Description |
|---------|----------|----------|-------------|
| [SLQ101](#slq101-distinct-call-is-redundant) | Redundancy | Warning | A `Distinct` call immediately before a `ToHashSet` call |
| [SLQ102](#slq102-redundant-element-selector) | Redundancy | Warning | An identity element selector (`x => x`) on `ToDictionary`, `ToLookup`, or `GroupBy` |
| [SLQ103](#slq103-select-call-is-redundant) | Redundancy | Warning | A `Select` call with an identity selector (`x => x`) |
| [SLQ201](#slq201-select-can-be-simplified) | Simplification | Warning | A `Select` that only yields the element and its index |
| [SLQ202](#slq202-orderby-can-be-simplified) | Simplification | Warning | An `OrderBy` with the identity key selector (`x => x`) |

### SLQ101: Distinct call is redundant

`ToHashSet` already removes duplicate elements, so a `Distinct` call immediately before it does no useful work. 
The rule only reports when both calls use the same equality comparer (both the default, or the same `StringComparer` member).

```csharp
// Before
// SLQ101: The Distinct call is redundant; the following ToHashSet call removes duplicates
return items.Distinct().ToHashSet();

// After
return items.ToHashSet();
```

### SLQ102: Redundant element selector

`ToDictionary`, `ToLookup`, and `GroupBy` all have overloads without an element selector that assume the element is the source item itself. 
An element selector of `x => x` is redundant and can be removed.

```csharp
// Before
// SLQ102: The element selector (x => x) is redundant and can be removed
var byLength = words.ToDictionary(x => x.Length, x => x);

// After
var byLength = words.ToDictionary(x => x.Length);
```

### SLQ103: Select call is redundant

A `Select` whose selector simply returns the source element (`x => x`) does not transform the sequence and can be removed from the chain.

```csharp
// Before
// SLQ103: The identity element selector makes the Select call redundant and it can be removed
var nonEmpty = items.Where(x => x.Length > 0).Select(x => x);

// After
var nonEmpty = items.Where(x => x.Length > 0);
```

### SLQ201: Select can be simplified

A `Select` that only returns the element and its index, such as `(x, i) => (x, i)` or `(x, i) => new { x, i }`, can be replaced with the more concise `Index` method, 
available in .NET 9 and later. The rule only applies to projects targeting .NET 9+. Note that `Index` yields the index before the element.

```csharp
// Before
// SLQ202: Select can be simplified to Index()
var indexed = items.Select((item, index) => (index, item));

// After
var indexed = items.Index();
```

### SLQ202: OrderBy can be simplified

An `OrderBy` with the identity function (`x => x`) sorts using the default comparer and is equivalent to the more concise `Order` method, available in .NET 7 and later. 
The rule only applies to projects targeting .NET 7+.

```csharp
// Before
// SLQ201: OrderBy can be simplified to Order without the (x => x)
var sorted = items.OrderBy(x => x);

// After
var sorted = items.Order();
```
