# Succinct LINQ

Succinct LINQ is a set of Roslyn analyzers for C# that flag redundant and verbose LINQ expressions and suggest more concise equivalents.

Add the package to any C# project and the rules run as part of the normal build. Every rule is enabled by default and reports as a warning.

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

| Rule ID | Category | Description |
|---------|----------|-------------|
| [SLQ101](#slq101-distinct-call-is-redundant) | Redundancy | A `Distinct` call immediately before a `ToHashSet` call |
| [SLQ102](#slq102-redundant-element-selector) | Redundancy | An identity element selector (`x => x`) on `ToDictionary`, `ToLookup`, or `GroupBy` |
| [SLQ103](#slq103-select-call-is-redundant) | Redundancy | A `Select` call with an identity selector (`x => x`) |
| [SLQ201](#slq201-select-can-be-simplified) | Simplification | A `Select` that only yields the element and its index |
| [SLQ202](#slq202-orderby-can-be-simplified) | Simplification | An `OrderBy` with the identity key selector (`x => x`) |
| [SLQ203](#slq203-orderbydescending-can-be-simplified) | Simplification | An `OrderByDescending` with the identity key selector (`x => x`) |
| [SLQ204](#slq204-orderby-and-firstordefault-can-be-simplified) | Simplification | An `OrderBy` followed by a `FirstOrDefault` |
| [SLQ205](#slq205-orderbydescending-and-firstordefault-can-be-simplified) | Simplification | An `OrderByDescending` followed by a `FirstOrDefault` |
| [SLQ206](#slq206-groupby-and-select-can-be-simplified) | Simplification | A `GroupBy` followed by a `Select` that takes the first element of each group |
| [SLQ207](#slq207-groupby-with-element-selector-and-select-can-be-simplified) | Simplification | A `GroupBy` with an element selector followed by a `Select` that takes the first element of each group |
| [SLQ208](#slq208-groupby-and-select-of-a-member-of-first-can-be-simplified) | Simplification | A `GroupBy` followed by a `Select` that takes a member of the first element of each group |

### SLQ101: Distinct call is redundant

Only reports when both calls use the same equality comparer (both the default, or the same `StringComparer` member).

```csharp
// Before
// SLQ101: Remove the redundant Distinct() call
return items.Distinct().ToHashSet();

// After
return items.ToHashSet();
```

### SLQ102: Redundant element selector

```csharp
// Before
// SLQ102: Remove the redundant "x => x" selector
var byLength = words.ToDictionary(x => x.Length, x => x);

// After
var byLength = words.ToDictionary(x => x.Length);
```

### SLQ103: Select call is redundant

```csharp
// Before
// SLQ103: Remove the redundant Select(x => x) call
var nonEmpty = items.Where(x => x.Length > 0).Select(x => x);

// After
var nonEmpty = items.Where(x => x.Length > 0);
```

### SLQ201: Select can be simplified

Only applies to projects targeting .NET 9+. Note that `Index` yields the index before the element, so the replacement may require adapting the code, such as `(x, i) => (i, x)`.

```csharp
// Before
// SLQ201: Use Index() instead
var indexed = items.Select((item, index) => (index, item));

// After
var indexed = items.Index();
```

### SLQ202: OrderBy can be simplified

Only applies to projects targeting .NET 7+.

```csharp
// Before
// SLQ202: Use Order() instead
var sorted = items.OrderBy(x => x);

// After
var sorted = items.Order();
```

### SLQ203: OrderByDescending can be simplified

Only applies to projects targeting .NET 7+.

```csharp
// Before
// SLQ203: Use OrderDescending() instead
var sorted = items.OrderByDescending(x => x);

// After
var sorted = items.OrderDescending();
```

### SLQ204: OrderBy and FirstOrDefault can be simplified

Only applies to projects targeting .NET 6+ and to sequences whose elements are nullable types and whose key type is a non-nullable value type.

```csharp
// Before
// SLQ204: Use MinBy(x => x.Length) instead
var shortest = items.OrderBy(x => x.Length).FirstOrDefault();

// After
var shortest = items.MinBy(x => x.Length);
```

### SLQ205: OrderByDescending and FirstOrDefault can be simplified

Only applies to projects targeting .NET 6+ and to sequences whose elements are nullable types. When a non-null custom comparer is supplied, the key type must also be a non-nullable value type.

```csharp
// Before
// SLQ205: Use MaxBy(x => x.Length) instead
var longest = items.OrderByDescending(x => x.Length).FirstOrDefault();

// After
var longest = items.MaxBy(x => x.Length);
```

### SLQ206: GroupBy and Select can be simplified

Only applies to projects targeting .NET 6+ and to `GroupBy` calls without an element selector.

```csharp
// Before
// SLQ206: Use DistinctBy(x => x.Id) instead
var unique = elements.GroupBy(x => x.Id).Select(x => x.First());

// After
var unique = elements.DistinctBy(x => x.Id);
```

### SLQ207: GroupBy with element selector and Select can be simplified

Only applies to projects targeting .NET 6+. Note that `GroupBy` invokes the element selector for every element, whereas the suggested `DistinctBy` followed by `Select` invokes it only for the first element of each distinct key, so if the selector has side effects or can throw, the refactored code may behave differently.

```csharp
// Before
// SLQ207: Use DistinctBy(x => x.Id).Select(x => x.ToString()) instead
var unique = elements.GroupBy(x => x.Id, x => x.ToString()).Select(x => x.First());

// After
var unique = elements.DistinctBy(x => x.Id).Select(x => x.ToString());
```

### SLQ208: GroupBy and Select of a member of First can be simplified

Only applies to projects targeting .NET 6+ and to `GroupBy` calls without an element selector.

```csharp
// Before
// SLQ208: Use DistinctBy(x => x.Id).Select(g => g.Value) instead
var unique = elements.GroupBy(x => x.Id).Select(g => g.First().Value);

// After
var unique = elements.DistinctBy(x => x.Id).Select(g => g.Value);
```
