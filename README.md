# SuccinctLinq

SuccinctLinq is a set of Roslyn analyzers for C# that flag redundant and verbose LINQ expressions and suggest more concise equivalents.

Add the package to any C# project and the rules run as part of the normal build.

## How to install

SuccinctLinq is available as a [package on nuget.org](https://www.nuget.org/packages/SuccinctLinq.Analyzers).

### .NET CLI

```powershell
dotnet add package SuccinctLinq.Analyzers
```

### Package Manager

```powershell
Install-Package SuccinctLinq.Analyzers
```

### Directory.Packages.props

```xml
<GlobalPackageReference Include="SuccinctLinq.Analyzers" Version="0.3.0" />
```

## Rules

Every rule is enabled by default and reports as a warning. Note that some rules might not emit any warning if the target framework of the project is below a certain level. For example, rule `SLQ204` requires the target framework to be at least .NET 6, because the suggested `MinBy(...)` replacement was introduced in .NET 6.

### Redundant `Distinct()` call

This rule suggests removing `Distinct()` calls that are immediately followed by `ToHashSet()`, since `ToHashSet()` already removes duplicates.

| Rule | Before | After |
| --- | --- | --- |
| SLQ101 | `Distinct().ToHashSet()` | `ToHashSet()` |

```diff
// Example that emits warning SLQ101:
- return items.Distinct().ToHashSet();

// Suggested replacement:
+ return items.ToHashSet();
```

If a comparer is used, both `Distinct()` and `ToHashSet()` must use the same comparer.
Otherwise the behavior of the code may change, and the analyzer will not suggest the replacement.

### Redundant identity element selector

This rule suggests removing `x => x` element selectors when there is an equivalent overload that does not require it.
The rule checks `GroupBy`, `ToDictionary`, and `ToLookup` calls.

| Rule | Before | After |
| --- | --- | --- |
| SLQ102 | `ToDictionary(a, x => x)` | `ToDictionary(a)` |

```diff
// Example that emits warning SLQ102:
- var byLength = words.GroupBy(x => x.Length, x => x);

// Suggested replacement:
+ var byLength = words.GroupBy(x => x.Length);
```

### Redundant `Select(x => x)` call

| Rule | Before | After |
| --- | --- | --- |
| SLQ103 | `items.Select(x => x)` | `items` |

```diff
// Example that emits warning SLQ103:
- var nonEmpty = items.Where(x => x.Length > 0).Select(x => x);

// Suggested replacement:
+ var nonEmpty = items.Where(x => x.Length > 0);
```

### Redundant `Key` and `Value` selectors in `ToDictionary`

These rules suggest removing redundant selectors in a `ToDictionary` call when there is an equivalent parameterless overload.

Target framework: .NET 8+

| Rule | Before | After |
| --- | --- | --- |
| SLQ104 | `keyValuePairs.ToDictionary(x => x.Key, x => x.Value)` | `keyValuePairs.ToDictionary()` |
| SLQ105 | `tuples.ToDictionary(x => x.Item1, x => x.Item2)` | `tuples.ToDictionary()` |

```diff
var tuples = new List<(int Id, string Name)>();

// Example that emits warning SLQ105:
- var dictionary = tuples.ToDictionary(x => x.Id, x => x.Name);

// Suggested replacement:
+ var dictionary = tuples.ToDictionary();
```

### Use `Index()` instead of `Select` with index parameter

`Select` has an overload that provides the index of each element.
If only a tuple of the element and index are needed, the more concise `Index()` method can be used instead.

Target framework: .NET 9+

| Rule | Before | After |
| --- | --- | --- |
| SLQ201 | `Select((x, i) => (i, x))` | `Index()` |


```diff
// Example that emits warning SLQ201:
- var indexed = items.Select((item, index) => (index, item));

// Suggested replacement:
+ var indexed = items.Index();
```

Note that `Index()` yields the index before the element, so the replacement may require changing the order of the tuple elements.

### Use `Order()` instead of `OrderBy(x => x)`

Ordering by the identity function `(x => x)` is equivalent to the more concise `Order()`.
Same goes for `OrderDescending()`.

Target framework: .NET 7+

| Rule | Before | After |
| --- | --- | --- |
| SLQ202 | `OrderBy(x => x)` | `Order()` |
| SLQ203 | `OrderByDescending(x => x)` | `OrderDescending()` |

```diff
// Example that emits warning SLQ203:
- var sorted = items.OrderByDescending(x => x);

// Suggested replacement:
+ var sorted = items.OrderDescending();
```


### Use `MinBy()`/`MaxBy()` instead of ordering followed by `FirstOrDefault()`

These rules suggest replacing ordering with a `MinBy()` or a `MaxBy()` when only the first element is needed.

Target framework: .NET 6+

| Rule | Before | After |
| --- | --- | --- |
| SLQ204 | `OrderBy(a).FirstOrDefault()` | `MinBy(a)` |
| SLQ205 | `OrderByDescending(a).FirstOrDefault()` | `MaxBy(a)` |

```diff
// Example that emits warning SLQ204:
- var shortest = items.OrderBy(x => x.Length).FirstOrDefault();

// Suggested replacement:
+ var shortest = items.MinBy(x => x.Length);
```

Note that this applies only to sequences whose elements are nullable types, because on an empty sequence of non-nullable value types the behavior would change: `MinBy`/`MaxBy` throw, while `FirstOrDefault` returns the default value.

The key type must also be a non-nullable value type.
For SLQ204 because `OrderBy` orders null keys first while `MinBy` ignores them.
For SLQ205 when a custom comparer is used, because the comparer may rank null keys above non-null keys.

### Use `DistinctBy()` instead of `GroupBy()` with `First()`

These rules suggest replacing a `GroupBy` that selects the first element of each group with a `DistinctBy` equivalent.

Target framework: .NET 6+

| Rule | Before | After |
| --- | --- | --- |
| SLQ206 | `GroupBy(a).Select(g => g.First())` | `DistinctBy(a)` |
| SLQ207 | `GroupBy(a, b).Select(g => g.First())` | `DistinctBy(a).Select(b)` |
| SLQ208 | `GroupBy(a).Select(g => g.First().b)` | `DistinctBy(a).Select(g => g.b)` |

```diff
// Example that emits warning SLQ206:
- var unique = elements.GroupBy(x => x.Id).Select(g => g.First());

// Suggested replacement:
+ var unique = elements.DistinctBy(x => x.Id);
```

Note that for the case covered by rule SLQ207, the `GroupBy` invokes the element selector (`b` in the table above) for every element.
However in the suggested `DistinctBy` replacement, the selector is invoked only for the first element of each distinct key.
If the selector has side effects or can throw, the refactored code may behave differently.

### Use `DistinctBy()` instead of `GroupBy()` when creating a dictionary of firsts

These rules suggest replacing a `GroupBy` followed by a `ToDictionary` that maps each key to the first element of its group (or a member of it) with a `DistinctBy` followed by a `ToDictionary`.

Target framework: .NET 6+

| Rule | Before | After |
| --- | --- | --- |
| SLQ209 | `GroupBy(a).ToDictionary(g => g.Key, g => g.First())` | `DistinctBy(a).ToDictionary(a)` |
| SLQ210 | `GroupBy(a, b).ToDictionary(g => g.Key, g => g.First())` | `DistinctBy(a).ToDictionary(a, b)` |
| SLQ211 | `GroupBy(a).ToDictionary(g => g.Key, g => g.First().m)` | `DistinctBy(a).ToDictionary(a, g => g.m)` |

```diff
// Example that emits warning SLQ209:
- var byId = items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First());

// Suggested replacement:
+ var byId = items.DistinctBy(x => x.Id).ToDictionary(x => x.Id);
```

Note that `GroupBy` evaluates the key selector once per element and the result is cached, while in the suggested replacement the key selector is evaluated twice.
The key selector must be a pure expression to avoid changing the behavior of the code.

### Use `Distinct()` instead of `DistinctBy(x => x)`

Distinct by the identity function `(x => x)` is equivalent to the more concise `Distinct()`.

Target framework: .NET 6+

| Rule | Before | After |
| --- | --- | --- |
| SLQ212 | `DistinctBy(x => x)` | `Distinct()` |

```diff
// Example that emits warning SLQ212:
- var unique = items.DistinctBy(x => x);

// Suggested replacement:
+ var unique = items.Distinct();
```

### Use `CountBy()` instead of `GroupBy()` when creating a dictionary of counts

Target framework: .NET 9+

| Rule | Before | After |
| --- | --- | --- |
| SLQ213 | `GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count())` | `CountBy(x => x.Id).ToDictionary()` |

```diff
// Example that emits warning SLQ213:
- var counts = items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count());

// Suggested replacement:
+ var counts = items.CountBy(x => x.Id).ToDictionary();
```
