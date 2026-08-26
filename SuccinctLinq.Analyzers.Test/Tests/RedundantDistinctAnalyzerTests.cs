using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class RedundantDistinctAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task RedundantDistinct_DistinctThenToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1001:Distinct()|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_ToHashSetWithoutDistinct_NoWarning()
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
    public Task RedundantDistinct_DistinctInLongerChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<int> MyMethod(IEnumerable<string> items)
                {
                    return items.Select(x => x.Length).{|SLQ1001:Distinct()|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_IntermediateOperation_NoWarning()
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
    public Task RedundantDistinct_DistinctThenToList_NoWarning()
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
    public Task RedundantDistinct_DistinctWithComparer_NoWarning()
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
    public Task RedundantDistinct_ToHashSetWithComparer_NoWarning()
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
    public Task RedundantDistinct_StaticDistinctInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ1001:Distinct(items)|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_FullyQualifiedDistinctInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ1001:Distinct(items)|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_StaticToHashSetInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.ToHashSet(Enumerable.{|SLQ1001:Distinct(items)|});
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_MultipleCalls_ReportWarningForEach()
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
                    var distinctNumbers = numbers.{|SLQ1001:Distinct()|}.ToHashSet();
                    var distinctWords = words.{|SLQ1001:Distinct()|}.ToHashSet();
                    return (distinctNumbers, distinctWords);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_ChainedDistinctToHashSetPairs_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1001:Distinct()|}.ToHashSet().{|SLQ1001:Distinct()|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_SameComparerInDistinctAndToHashSet_ReportWarning()
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
                    return items.{|SLQ1001:Distinct(comparer)|}.ToHashSet(comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_SameComparerInStaticDistinctAndToHashSet_ReportWarning()
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
                    return Enumerable.ToHashSet(Enumerable.{|SLQ1001:Distinct(items, comparer)|}, comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_SameComparerFieldInDistinctAndToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1001:Distinct(StringComparer.OrdinalIgnoreCase)|}.ToHashSet(StringComparer.OrdinalIgnoreCase);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_SameInstancePropertyComparerInDistinctAndToHashSet_ReportWarning()
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
                    return items.{|SLQ1001:Distinct(holder.Comparer)|}.ToHashSet(holder.Comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_DifferentInstancePropertyComparersInDistinctAndToHashSet_NoWarning()
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
    public Task RedundantDistinct_DifferentComparersInDistinctAndToHashSet_NoWarning()
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
    public Task RedundantDistinct_DistinctAndToHashSetWithNonComparerParameter_NoWarning()
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
    public Task RedundantDistinct_InstanceDistinctWithStaticToHashSetAndComparer_ReportWarning()
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
                    return Enumerable.ToHashSet(items.{|SLQ1001:Distinct(comparer)|}, comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_SameLocalComparerInDistinctAndToHashSet_ReportWarning()
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
                    return items.{|SLQ1001:Distinct(comparer)|}.ToHashSet(comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_DifferentLocalComparersInDistinctAndToHashSet_NoWarning()
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
    public Task RedundantDistinct_SameComparerFactoryInvocationInDistinctAndToHashSet_NoWarning()
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
    public Task RedundantDistinct_SameComparerCreationInDistinctAndToHashSet_NoWarning()
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
    public Task RedundantDistinct_SameConditionalComparerInDistinctAndToHashSet_NoWarning()
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
    public Task RedundantDistinct_SameNullComparerInDistinctAndToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1001:Distinct(null)|}.ToHashSet(null);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_SameDefaultComparerInDistinctAndToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1001:Distinct(default)|}.ToHashSet(default);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_NonLinqEnumerable_NoWarning()
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
    public Task RedundantDistinct_DistinctInLocalVariableThenToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    var distinctItems = items.{|SLQ1001:Distinct()|};
                    return distinctItems.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_StaticToHashSetOnDistinctInLocalVariable_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    var distinctItems = items.{|SLQ1001:Distinct()|};
                    return Enumerable.ToHashSet(distinctItems);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_SameComparerInDistinctInLocalVariableAndToHashSet_ReportWarning()
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
                    var distinctItems = items.{|SLQ1001:Distinct(comparer)|};
                    return distinctItems.ToHashSet(comparer);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_DistinctReassignedToInitializedLocalVariableThenToHashSet_ReportWarning()
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
                    d = items.{|SLQ1001:Distinct()|};
                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_DistinctReassignedToUninitializedLocalVariableThenToHashSet_ReportWarning()
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
                    d = items.{|SLQ1001:Distinct()|};
                    return d.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_DistinctReassignedToLocalVariableWithAdditionalUse_NoWarning()
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
    public Task RedundantDistinct_NonDistinctReassignmentAfterDistinct_NoWarning()
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
    public Task RedundantDistinct_DistinctInLocalVariableWithAdditionalUse_NoWarning()
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
    public Task RedundantDistinct_DistinctInLocalVariableWithDifferentComparers_NoWarning()
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
    public Task RedundantDistinct_DistinctInLocalVariableUsedInTwoToHashSetCalls_NoWarning()
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
    public Task RedundantDistinct_NonDistinctLocalVariableThenToHashSet_NoWarning()
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
    public Task RedundantDistinct_DistinctAssignedInBranchThenToHashSetOutside_NoWarning()
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
    public Task RedundantDistinct_DistinctAssignedInTernaryThenToHashSetOutside_NoWarning()
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
    public Task RedundantDistinct_DistinctAssignedInLoopThenToHashSetOutside_NoWarning()
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
    public Task RedundantDistinct_DistinctAssignedInBranchWithToHashSetInSameBranch_ReportWarning()
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
                        d = items.{|SLQ1001:Distinct()|};
                        d.ToHashSet();
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_DistinctAssignedInBranchThenToHashSetInSiblingBranch_NoWarning()
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
    public Task RedundantDistinct_DistinctInTryBodyThenToHashSetInCatch_NoWarning()
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
    public Task RedundantDistinct_DistinctInTryBodyThenToHashSetAfterTry_NoWarning()
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
    public Task RedundantDistinct_DistinctAndToHashSetInTryBody_ReportWarning()
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
                        d = items.{|SLQ1001:Distinct()|};
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
    public Task RedundantDistinct_DistinctInOneSwitchCaseThenToHashSetInAnotherCase_NoWarning()
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
    public Task RedundantDistinct_DistinctAndToHashSetInSameSwitchCase_ReportWarning()
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
                            d = items.{|SLQ1001:Distinct()|};
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
    public Task RedundantDistinct_DistinctAssignedInAndAlsoSecondOperandThenToHashSetOutside_NoWarning()
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
    public Task RedundantDistinct_DistinctAssignedInOrElseSecondOperandThenToHashSetOutside_NoWarning()
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
    public Task RedundantDistinct_WriteInUncalledLambdaThenToHashSet_NoWarning()
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
    public Task RedundantDistinct_WriteInAsyncLambdaThenToHashSet_NoWarning()
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
    public Task RedundantDistinct_WriteInUncalledLocalFunctionThenToHashSet_NoWarning()
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
    public Task RedundantDistinct_ToHashSetInLambdaThenLocalReassigned_NoWarning()
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
    public Task RedundantDistinct_ReadInLambdaThenToHashSet_NoWarning()
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
