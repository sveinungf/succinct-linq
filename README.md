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

### SLQ206: Use DistinctBy(a) instead of GroupBy(a).Select(x => x.First())

Only applies to projects targeting .NET 6+.

```csharp
// Before
// SLQ206: Use DistinctBy(x => x.Id) instead
var unique = elements.GroupBy(x => x.Id).Select(x => x.First());

// After
var unique = elements.DistinctBy(x => x.Id);
```

### SLQ207: Use DistinctBy(a).Select(b) instead of GroupBy(a, b).Select(x => x.First())

Only applies to projects targeting .NET 6+. Note that `GroupBy` invokes the element selector for every element, whereas the suggested `DistinctBy` followed by `Select` invokes it only for the first element of each distinct key. If the selector has side effects or can throw, the refactored code may behave differently.

```csharp
// Before
// SLQ207: Use DistinctBy(x => x.Id).Select(x => x.ToString()) instead
var unique = elements.GroupBy(x => x.Id, x => x.ToString()).Select(x => x.First());

// After
var unique = elements.DistinctBy(x => x.Id).Select(x => x.ToString());
```

### SLQ208: Use DistinctBy(a).Select(x => x.b) instead of GroupBy(a).Select(x => x.First().b)

Only applies to projects targeting .NET 6+ and to `GroupBy` calls without an element selector.

```csharp
// Before
// SLQ208: Use DistinctBy(x => x.Id).Select(g => g.Value) instead
var unique = elements.GroupBy(x => x.Id).Select(g => g.First().Value);

// After
var unique = elements.DistinctBy(x => x.Id).Select(g => g.Value);
```
