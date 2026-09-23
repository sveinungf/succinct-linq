using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class OrderByIdentityKeyAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task OrderBy_IdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:OrderBy(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_IdentityLambdaInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ202:OrderBy(x => x)|}.Select(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_IdentityLambdaWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:OrderBy(x => x, StringComparer.Ordinal)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ202:OrderBy(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_StaticInvocationOutOfOrderNamedArguments_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ202:OrderBy(keySelector: x => x, source: items)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_OutOfOrderNamedArguments_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:OrderBy(comparer: StringComparer.Ordinal, keySelector: x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_FullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ202:OrderBy(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_IdentityLambdaWithSameTypeCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:OrderBy(x => (string)x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_ValueChangingCast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<double> MyMethod(IEnumerable<double> items)
                {
                    return items.OrderBy(x => (double)(int)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_GenericSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<T> MyMethod<T>(IEnumerable<T> items)
                {
                    return items.{|SLQ202:OrderBy(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_MultipleCalls_ReportWarningForEach()
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
                    var orderedWords = words.{|SLQ202:OrderBy(x => x)|};
                    var orderedNumbers = numbers.{|SLQ202:OrderBy(x => x)|};
                    return (orderedWords, orderedNumbers);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_DifferentKeySelector_NoWarning()
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
    public Task OrderBy_DifferentKeyType_NoWarning()
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
    public Task OrderBy_NullForgivingKeySelectorOnNullableValueSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<int?> MyMethod(IEnumerable<int?> items)
                {
                    return items.{|SLQ202:OrderBy(x => x!)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_NullForgivingKeySelectorOnNullableReferenceSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string?> MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ202:OrderBy(x => x!)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_IdentityLambdaOnNullableSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<int?> MyMethod(IEnumerable<int?> items)
                {
                    return items.{|SLQ202:OrderBy(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_StatementBodyIdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:OrderBy(x => { return x; })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_MethodGroupKeySelector_NoWarning()
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
    public Task OrderByDescending_IdentityLambda_NoWarning()
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
    public Task ThenBy_IdentityLambda_NoWarning()
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
    public Task Order_NoWarning()
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
    public Task OrderBy_WithNonKeySelectorParameter_NoWarning()
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
    public Task OrderBy_TargetFrameworkBeforeNet7_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net60);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_TargetFrameworkNetStandard_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_TargetFrameworkNet7_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByIdentityKeyAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net70);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:OrderBy(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithNonComparerParameter_NoWarning()
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
