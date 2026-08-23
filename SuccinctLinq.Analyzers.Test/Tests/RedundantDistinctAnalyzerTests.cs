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
}
