using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class OrderByIdentityKeyAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task OrderByIdentityKey_IdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1101:OrderBy(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_IdentityLambdaInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ1101:OrderBy(x => x)|}.Select(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_IdentityLambdaWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1101:OrderBy(x => x, StringComparer.Ordinal)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_StaticOrderByInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ1101:OrderBy(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_FullyQualifiedOrderByInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ1101:OrderBy(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_IdentityLambdaWithSameTypeCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1101:OrderBy(x => (string)x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_GenericSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<T> MyMethod<T>(IEnumerable<T> items)
                {
                    return items.{|SLQ1101:OrderBy(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_MultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (IOrderedEnumerable<string> Words, IOrderedEnumerable<int> Numbers) MyMethod(
                    IEnumerable<string> words, IEnumerable<int> numbers)
                {
                    var orderedWords = words.{|SLQ1101:OrderBy(x => x)|};
                    var orderedNumbers = numbers.{|SLQ1101:OrderBy(x => x)|};
                    return (orderedWords, orderedNumbers);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_DifferentKeySelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_DifferentKeyType_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => (object)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_StatementBodyIdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1101:OrderBy(x => { return x; })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_MethodGroupKeySelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(Identity);
                }

                public static T Identity<T>(T value) => value;
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_DescendingIdentityLambda_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_ThenByWithIdentityLambda_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length).ThenBy(x => x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_Order_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Order();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_OrderByWithNonKeySelectorParameter_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace System.Linq
            {
                public static class Enumerable
                {
                    public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> items, Marker marker) => items;
                }
            }

            public sealed class Marker
            {
            }

            namespace MyNamespace
            {
                public static class MyClass
                {
                    public static IEnumerable<string> MyMethod(IEnumerable<string> items, Marker marker)
                    {
                        return items.OrderBy(marker);
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByIdentityKey_OrderByWithNonComparerParameter_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace System.Linq
            {
                public static class Enumerable
                {
                    public static IEnumerable<T> OrderBy<T>(
                        this IEnumerable<T> items, Func<T, T> keySelector, Marker marker) => items;
                }
            }

            public sealed class Marker
            {
            }

            namespace MyNamespace
            {
                public static class MyClass
                {
                    public static IEnumerable<string> MyMethod(IEnumerable<string> items, Marker marker)
                    {
                        return items.OrderBy(x => x, marker);
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
