# Succinct LINQ

Succinct LINQ is a set of Roslyn analyzers for C# that flag redundant and verbose LINQ expressions and suggest more concise equivalents.

Add the package to any C# project and the rules run as part of the normal build.

## How to install

Succinct LINQ is available as a [package on nuget.org](https://www.nuget.org/packages/SuccinctLinq.Analyzers).

### .NET CLI

```powershell
dotnet add package SuccinctLinq.Analyzers
```

### Package Manager

```powershell
Install-Package SuccinctLinq.Analyzers
```

### Directory.Packages.props (Central Package Management)

```xml
<GlobalPackageReference Include="SuccinctLinq.Analyzers" Version="0.2.0" />
```

## Rules

Every rule is enabled by default and reports as a warning. Note that some rules might not emit any warning if the target framework of the project is below a certain level. For example, rule `SLQ204` requires the target framework to be at least .NET 6, because the suggested `MinBy(...)` replacement was introduced in .NET 6.

### SLQ101: Distinct() before ToHashSet() is redundant

Only reports when both calls use the same equality comparer (both the default, or the same `StringComparer` member).

```csharp
// Before
// SLQ101: Remove the redundant Distinct() call
return items.Distinct().ToHashSet();

// After
return items.ToHashSet();
```

### SLQ102: Redundant "x => x" element selector

```csharp
// Before
// SLQ102: Remove the redundant "x => x" selector
var byLength = words.ToDictionary(x => x.Length, x => x);

// After
var byLength = words.ToDictionary(x => x.Length);
```

### SLQ103: Select(x => x) is redundant

```csharp
// Before
// SLQ103: Remove the redundant Select(x => x) call
var nonEmpty = items.Where(x => x.Length > 0).Select(x => x);

// After
var nonEmpty = items.Where(x => x.Length > 0);
```

### SLQ104, SLQ105: Redundant Key and Value selectors in ToDictionary

Only applies to projects targeting .NET 8+. These rules suggest replacing a `ToDictionary` call that selects each element's key and value with the parameterless `ToDictionary()` overload:

| Rule | Before | After |
| --- | --- | --- |
| SLQ104 | `pairs.ToDictionary(x => x.Key, x => x.Value)` | `pairs.ToDictionary()` |
| SLQ105 | `tuples.ToDictionary(x => x.Item1, x => x.Item2)` | `tuples.ToDictionary()` |

```csharp
// Before
// SLQ104: Simplify to just ToDictionary()
var dictionary = pairs.ToDictionary(x => x.Key, x => x.Value);

// After
var dictionary = pairs.ToDictionary();
```

### SLQ201: Use Index() instead of Select with index parameter

Only applies to projects targeting .NET 9+. Note that `Index` yields the index before the element, so the replacement may require adapting the code, such as `(x, i) => (i, x)`.

```csharp
// Before
// SLQ201: Use Index() instead
var indexed = items.Select((item, index) => (index, item));

// After
var indexed = items.Index();
```

### SLQ202: Use Order() instead of OrderBy(x => x)

Only applies to projects targeting .NET 7+.

```csharp
// Before
// SLQ202: Use Order() instead
var sorted = items.OrderBy(x => x);

// After
var sorted = items.Order();
```

### SLQ203: Use OrderDescending() instead of OrderByDescending(x => x)

Only applies to projects targeting .NET 7+.

```csharp
// Before
// SLQ203: Use OrderDescending() instead
var sorted = items.OrderByDescending(x => x);

// After
var sorted = items.OrderDescending();
```

### SLQ204: Use MinBy(a) instead of OrderBy(a).FirstOrDefault()

Only applies to projects targeting .NET 6+ and to sequences whose elements are nullable types and whose key type is a non-nullable value type.

```csharp
// Before
// SLQ204: Use MinBy(x => x.Length) instead
var shortest = items.OrderBy(x => x.Length).FirstOrDefault();

// After
var shortest = items.MinBy(x => x.Length);
```

### SLQ205: Use MaxBy(a) instead of OrderByDescending(a).FirstOrDefault()

Only applies to projects targeting .NET 6+ and to sequences whose elements are nullable types. When a non-null custom comparer is supplied, the key type must also be a non-nullable value type.

```csharp
// Before
// SLQ205: Use MaxBy(x => x.Length) instead
var longest = items.OrderByDescending(x => x.Length).FirstOrDefault();

// After
var longest = items.MaxBy(x => x.Length);
```

### SLQ206, SLQ207, SLQ208: Use DistinctBy() instead of GroupBy()

Only applies to projects targeting .NET 6+. These rules suggest replacing a `GroupBy` that selects the first element of each group with a `DistinctBy` equivalent:

| Rule | Before | After |
| --- | --- | --- |
| SLQ206 | `GroupBy(a).Select(g => g.First())` | `DistinctBy(a)` |
| SLQ207 | `GroupBy(a, b).Select(g => g.First())` | `DistinctBy(a).Select(b)` |
| SLQ208 | `GroupBy(a).Select(g => g.First().b)` | `DistinctBy(a).Select(g => g.b)` |

```csharp
// Before
// SLQ206: Use DistinctBy(x => x.Id) instead
var unique = elements.GroupBy(x => x.Id).Select(g => g.First());

// After
var unique = elements.DistinctBy(x => x.Id);
```

Note that `GroupBy` invokes the element selector for every element, whereas the suggested `DistinctBy` followed by `Select` invokes it only for the first element of each distinct key. If the selector has side effects or can throw, the refactored code may behave differently.

### SLQ209, SLQ210, SLQ211: Use DistinctBy() and ToDictionary() instead of GroupBy() and ToDictionary()

Only applies to projects targeting .NET 6+ and to key selectors that are pure expressions. These rules suggest replacing a `GroupBy` followed by a `ToDictionary` that maps each key to the first element of its group (or a member of it) with a `DistinctBy` followed by a `ToDictionary`:

| Rule | Before | After |
| --- | --- | --- |
| SLQ209 | `GroupBy(a).ToDictionary(g => g.Key, g => g.First())` | `DistinctBy(a).ToDictionary(a)` |
| SLQ210 | `GroupBy(a, b).ToDictionary(g => g.Key, g => g.First())` | `DistinctBy(a).ToDictionary(a, b)` |
| SLQ211 | `GroupBy(a).ToDictionary(g => g.Key, g => g.First().m)` | `DistinctBy(a).ToDictionary(a, g => g.m)` |

For SLQ210, when the element selector is the identity, the `ToDictionary` element selector can be omitted: `GroupBy(a, x => x).ToDictionary(g => g.Key, g => g.First())` simplifies to `DistinctBy(a).ToDictionary(a)`.

```csharp
// Before
// SLQ209: Use DistinctBy(x => x.Id).ToDictionary(x => x.Id) instead
var byId = items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First());

// After
var byId = items.DistinctBy(x => x.Id).ToDictionary(x => x.Id);
```

Note that `GroupBy` evaluates the key selector once per element and caches the result, whereas the suggested `DistinctBy` followed by `ToDictionary` evaluates it twice. This is why the key selector must be a pure expression.

### SLQ212: Use Distinct() instead of DistinctBy(x => x)

Only applies to projects targeting .NET 6+.

```csharp
// Before
// SLQ212: Use Distinct() instead
var unique = items.DistinctBy(x => x);

// After
var unique = items.Distinct();
```

### SLQ213: Use CountBy() and ToDictionary() instead of GroupBy() and ToDictionary()

Only applies to projects targeting .NET 9+.

```csharp
// Before
// SLQ213: Use CountBy(x => x.Id).ToDictionary() instead
var counts = items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count());

// After
var counts = items.CountBy(x => x.Id).ToDictionary();
```
