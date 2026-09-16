using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class RedundantDistinctAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task Distinct_ThenToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ101:Distinct()|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToHashSet_WithoutDistinct_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InLongerChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<int> MyMethod(IEnumerable<string> items)
                {
                    return items.Select(x => x.Length).{|SLQ101:Distinct()|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_IntermediateOperation_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct().Where(x => x.Length > 0).ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_ThenToList_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static List<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct().ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_WithComparer_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToHashSet_WithComparer_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ101:Distinct(items)|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_FullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ101:Distinct(items)|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_StaticToHashSetInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.ToHashSet(Enumerable.{|SLQ101:Distinct(items)|});
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_MultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (HashSet<int> Numbers, HashSet<string> Words) MyMethod(
                    IEnumerable<int> numbers, IEnumerable<string> words)
                {
                    var distinctNumbers = numbers.{|SLQ101:Distinct()|}.ToHashSet();
                    var distinctWords = words.{|SLQ101:Distinct()|}.ToHashSet();
                    return (distinctNumbers, distinctWords);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_ChainedToHashSetPairs_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ101:Distinct()|}.ToHashSet().{|SLQ101:Distinct()|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameParameterComparerInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(
                    IEnumerable<string> items, IEqualityComparer<string> comparer)
                {
                    return items.Distinct(comparer).ToHashSet(comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameParameterComparerInStaticToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(
                    IEnumerable<string> items, IEqualityComparer<string> comparer)
                {
                    return Enumerable.ToHashSet(Enumerable.Distinct(items, comparer), comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameStringComparerPropertyInToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ101:Distinct(StringComparer.OrdinalIgnoreCase)|}.ToHashSet(StringComparer.OrdinalIgnoreCase);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_DifferentStringComparerPropertiesInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.OrdinalIgnoreCase);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameInstancePropertyComparerInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public sealed class ComparerHolder
            {
                public IEqualityComparer<string> Comparer { get; } = StringComparer.OrdinalIgnoreCase;
            }

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items, ComparerHolder holder)
                {
                    return items.Distinct(holder.Comparer).ToHashSet(holder.Comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_DifferentInstancePropertyComparersInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public sealed class ComparerHolder
            {
                public IEqualityComparer<string> Comparer { get; } = StringComparer.OrdinalIgnoreCase;
            }

            public static class MyClass
            {
                public static HashSet<string> MyMethod(
                    IEnumerable<string> items, ComparerHolder a, ComparerHolder b)
                {
                    return items.Distinct(a.Comparer).ToHashSet(b.Comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_DifferentParameterComparersInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(
                    IEnumerable<string> items,
                    IEqualityComparer<string> distinctComparer,
                    IEqualityComparer<string> toHashSetComparer)
                {
                    return items.Distinct(distinctComparer).ToHashSet(toHashSetComparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_WithNonComparerParameter_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace System.Linq
            {
                public static class Enumerable
                {
                    public static IEnumerable<T> Distinct<T>(this IEnumerable<T> items, Marker marker) => items;
                    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items, Marker marker) => new HashSet<T>();
                }
            }

            public sealed class Marker
            {
            }

            namespace MyNamespace
            {
                public static class MyClass
                {
                    public static HashSet<string> MyMethod(IEnumerable<string> items, Marker marker)
                    {
                        return items.Distinct(marker).ToHashSet(marker);
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_WithStaticToHashSetAndParameterComparer_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(
                    IEnumerable<string> items, IEqualityComparer<string> comparer)
                {
                    return Enumerable.ToHashSet(items.Distinct(comparer), comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameLocalComparerInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    var comparer = StringComparer.OrdinalIgnoreCase;
                    return items.Distinct(comparer).ToHashSet(comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_DifferentLocalComparersInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    var distinctComparer = StringComparer.Ordinal;
                    var toHashSetComparer = StringComparer.OrdinalIgnoreCase;
                    return items.Distinct(distinctComparer).ToHashSet(toHashSetComparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameComparerFactoryInvocationInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                private static IEqualityComparer<string> CreateComparer() => StringComparer.OrdinalIgnoreCase;

                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct(CreateComparer()).ToHashSet(CreateComparer());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameComparerCreationInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public sealed class MyComparer : IEqualityComparer<string>
            {
                public bool Equals(string? x, string? y) => string.Equals(x, y);
                public int GetHashCode(string obj) => obj.GetHashCode();
            }

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct(new MyComparer()).ToHashSet(new MyComparer());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameConditionalComparerInToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items, bool useOrdinal)
                {
                    return items
                        .Distinct(useOrdinal ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(useOrdinal ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameNullComparerInToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ101:Distinct(null)|}.ToHashSet(null);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_SameDefaultComparerInToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ101:Distinct(default)|}.ToHashSet(default);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_NonLinqEnumerable_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class Enumerable
            {
                public static IEnumerable<T> Distinct<T>(this IEnumerable<T> items) => items;
                public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items) => new HashSet<T>();
            }

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct().ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InLocalVariableThenToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    var distinctItems = items.{|SLQ101:Distinct()|};
                    return distinctItems.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InLocalVariableThenStaticToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    var distinctItems = items.{|SLQ101:Distinct()|};
                    return Enumerable.ToHashSet(distinctItems);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InLocalVariableWithSameParameterComparer_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(
                    IEnumerable<string> items, IEqualityComparer<string> comparer)
                {
                    var distinctItems = items.Distinct(comparer);
                    return distinctItems.ToHashSet(comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_ReassignedToInitializedLocalVariableThenToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items;
                    d = items.{|SLQ101:Distinct()|};
                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_ReassignedToUninitializedLocalVariableThenToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d;
                    d = items.{|SLQ101:Distinct()|};
                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_ReassignedToLocalVariableWithAdditionalUse_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items;
                    d = items.Distinct();
                    Console.WriteLine(d.Count());
                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_NonDistinctReassignmentAfter_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items.Distinct();
                    d = items;
                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InLocalVariableWithAdditionalUse_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    var distinctItems = items.Distinct();
                    Console.WriteLine(distinctItems.Count());
                    return distinctItems.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InLocalVariableWithDifferentParameterComparers_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(
                    IEnumerable<string> items,
                    IEqualityComparer<string> distinctComparer,
                    IEqualityComparer<string> toHashSetComparer)
                {
                    var distinctItems = items.Distinct(distinctComparer);
                    return distinctItems.ToHashSet(toHashSetComparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InLocalVariableUsedInTwoToHashSetCalls_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (HashSet<string> First, HashSet<string> Second) MyMethod(
                    IEnumerable<string> items)
                {
                    var distinctItems = items.Distinct();
                    var first = distinctItems.ToHashSet();
                    var second = distinctItems.ToHashSet();
                    return (first, second);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_NonDistinctLocalVariableThenToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    var distinctItems = items.ToList();
                    return distinctItems.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_AssignedInBranchThenToHashSetOutside_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items, bool condition)
                {
                    IEnumerable<string> d;
                    if (condition)
                    {
                        d = items;
                    }
                    else
                    {
                        d = items.Distinct();
                    }

                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_AssignedInTernaryThenToHashSetOutside_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items, bool condition)
                {
                    IEnumerable<string> d = condition ? items.Distinct() : items;
                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_AssignedInLoopThenToHashSetOutside_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items, int count)
                {
                    IEnumerable<string> d = items;
                    for (var i = 0; i < count; i++)
                    {
                        d = items.Distinct();
                    }

                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_AssignedInBranchWithToHashSetInSameBranch_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items, bool condition)
                {
                    IEnumerable<string> d = items;
                    if (condition)
                    {
                        d = items.{|SLQ101:Distinct()|};
                        d.ToHashSet();
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_AssignedInBranchThenToHashSetInSiblingBranch_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items, bool condition)
                {
                    IEnumerable<string> d = items;
                    if (condition)
                    {
                        d = items.Distinct();
                    }
                    else
                    {
                        d.ToHashSet();
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InTryBodyThenToHashSetInCatch_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items;
                    try
                    {
                        d = items.Distinct();
                    }
                    catch
                    {
                        d.ToHashSet();
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InTryBodyThenToHashSetAfterTry_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items;
                    try
                    {
                        d = items.Distinct();
                    }
                    catch
                    {
                    }

                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_WithToHashSetInTryBody_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items)
                {
                    try
                    {
                        IEnumerable<string> d = items;
                        d = items.{|SLQ101:Distinct()|};
                        d.ToHashSet();
                    }
                    catch
                    {
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_InOneSwitchCaseThenToHashSetInAnotherCase_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items, int key)
                {
                    IEnumerable<string> d = items;
                    switch (key)
                    {
                        case 1:
                            d = items.Distinct();
                            break;
                        case 2:
                            d.ToHashSet();
                            break;
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_WithToHashSetInSameSwitchCase_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items, int key)
                {
                    switch (key)
                    {
                        case 1:
                            IEnumerable<string> d = items;
                            d = items.{|SLQ101:Distinct()|};
                            d.ToHashSet();
                            break;
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_AssignedInAndAlsoSecondOperandThenToHashSetOutside_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items, bool condition)
                {
                    IEnumerable<string> d = items;
                    var result = condition && ((d = items.Distinct()) is not null);
                    d.ToHashSet();
                    Console.WriteLine(result);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_AssignedInOrElseSecondOperandThenToHashSetOutside_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items, bool condition)
                {
                    IEnumerable<string> d = items;
                    var result = condition || ((d = items.Distinct()) is not null);
                    d.ToHashSet();
                    Console.WriteLine(result);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_WriteInUncalledLambdaThenToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items;
                    Func<int> f = () => { d = items.Distinct(); return 0; };
                    d.ToHashSet();
                    f();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_WriteInAsyncLambdaThenToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items;
                    Func<Task> f = async () => d = items.Distinct();
                    f();
                    d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_WriteInUncalledLocalFunctionThenToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items;
                    void Assign() => d = items.Distinct();
                    d.ToHashSet();
                    Assign();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_ToHashSetInLambdaThenLocalReassigned_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items.Distinct();
                    Func<HashSet<string>> f = () => d.ToHashSet();
                    d = items;
                    f();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_ReadInLambdaThenToHashSet_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static void MyMethod(IEnumerable<string> items)
                {
                    IEnumerable<string> d = items.Distinct();
                    Func<List<string>> f = () => d.ToList();
                    d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
