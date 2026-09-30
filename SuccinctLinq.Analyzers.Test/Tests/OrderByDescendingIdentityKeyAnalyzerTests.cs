using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class OrderByDescendingIdentityKeyAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task OrderByDescending_IdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ203:OrderByDescending(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_IdentityLambdaInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ203:OrderByDescending(x => x)|}.Select(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_IdentityLambdaWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ203:OrderByDescending(x => x, StringComparer.Ordinal)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_IdentityLambdaWithComparer_MessageMentionsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x, StringComparer.Ordinal);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ203", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 22, 7, 71)
                .WithMessage("Use OrderDescending(comparer) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ203:OrderByDescending(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_StaticInvocationOutOfOrderNamedArguments_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ203:OrderByDescending(keySelector: x => x, source: items)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_OutOfOrderNamedArguments_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ203:OrderByDescending(comparer: StringComparer.Ordinal, keySelector: x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_FullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ203:OrderByDescending(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_IdentityLambdaWithSameTypeCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ203:OrderByDescending(x => (string)x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_ValueChangingCast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<double> MyMethod(IEnumerable<double> items)
                {
                    return items.OrderByDescending(x => (double)(int)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_GenericSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<T> MyMethod<T>(IEnumerable<T> items)
                {
                    return items.{|SLQ203:OrderByDescending(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_MultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (IOrderedEnumerable<string> Words, IOrderedEnumerable<int> Numbers) MyMethod(
                    IEnumerable<string> words, IEnumerable<int> numbers)
                {
                    var orderedWords = words.{|SLQ203:OrderByDescending(x => x)|};
                    var orderedNumbers = numbers.{|SLQ203:OrderByDescending(x => x)|};
                    return (orderedWords, orderedNumbers);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_DifferentKeySelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_DifferentKeyType_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => (object)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_NullForgivingKeySelectorOnNullableValueSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<int?> MyMethod(IEnumerable<int?> items)
                {
                    return items.{|SLQ203:OrderByDescending(x => x!)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_NullForgivingKeySelectorOnNullableReferenceSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string?> MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ203:OrderByDescending(x => x!)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_IdentityLambdaOnNullableSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<int?> MyMethod(IEnumerable<int?> items)
                {
                    return items.{|SLQ203:OrderByDescending(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_StatementBodyIdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ203:OrderByDescending(x => { return x; })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_MethodGroupKeySelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(Identity);
                }

                public static T Identity<T>(T value) => value;
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_IdentityLambda_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
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
    public Task ThenByDescending_IdentityLambda_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length).ThenByDescending(x => x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderDescending_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderDescending();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithNonKeySelectorParameter_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace System.Linq
            {
                public static class Enumerable
                {
                    public static IEnumerable<T> OrderByDescending<T>(this IEnumerable<T> items, Marker marker) => items;
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
                        return items.OrderByDescending(marker);
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_TargetFrameworkBeforeNet7_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net60);
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
    public Task OrderByDescending_TargetFrameworkNetStandard_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
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
    public Task OrderByDescending_TargetFrameworkNet7_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net70);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ203:OrderByDescending(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithNonComparerParameter_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingIdentityKeyAnalyzer>();
        context.TestCode = """
            namespace System.Linq
            {
                public static class Enumerable
                {
                    public static IEnumerable<T> OrderByDescending<T>(
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
                        return items.OrderByDescending(x => x, marker);
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
