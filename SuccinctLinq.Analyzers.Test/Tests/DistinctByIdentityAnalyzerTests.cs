using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class DistinctByIdentityAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task DistinctBy_IdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ212:DistinctBy(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_IdentityLambdaInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ212:DistinctBy(x => x)|}.Select(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_IdentityLambda_MessageSuggestsDistinct()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.DistinctBy(x => x);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ212", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 22, 7, 40)
                .WithMessage("Use Distinct() instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ212:DistinctBy(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_FullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ212:DistinctBy(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_NamedArgumentsOutOfOrder_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ212:DistinctBy(keySelector: x => x, source: items)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_SameTypeCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ212:DistinctBy(x => (string)x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_ValueChangingCast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<double> MyMethod(IEnumerable<double> items)
                {
                    return items.DistinctBy(x => (double)(int)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_GenericSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<T> MyMethod<T>(IEnumerable<T> items)
                {
                    return items.{|SLQ212:DistinctBy(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_MultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (IEnumerable<string> Words, IEnumerable<int> Numbers) MyMethod(
                    IEnumerable<string> words, IEnumerable<int> numbers)
                {
                    var distinctWords = words.{|SLQ212:DistinctBy(x => x)|};
                    var distinctNumbers = numbers.{|SLQ212:DistinctBy(x => x)|};
                    return (distinctWords, distinctNumbers);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_StatementBodyIdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ212:DistinctBy(x => { return x; })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_MethodGroupKeySelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.DistinctBy(Identity);
                }

                public static T Identity<T>(T value) => value;
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_DifferentKeySelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.DistinctBy(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_DifferentKeyType_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.DistinctBy(x => (object)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_WithComparer_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.DistinctBy(x => x, StringComparer.Ordinal);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_WithNullComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ212:DistinctBy(x => x, null)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_NullForgivingKeySelectorOnNullableValueSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int?> MyMethod(IEnumerable<int?> items)
                {
                    return items.{|SLQ212:DistinctBy(x => x!)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_NullForgivingKeySelectorOnNullableReferenceSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string?> MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ212:DistinctBy(x => x!)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_IdentityLambdaOnNullableSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int?> MyMethod(IEnumerable<int?> items)
                {
                    return items.{|SLQ212:DistinctBy(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Distinct_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task DistinctBy_WithNonKeySelectorParameter_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<DistinctByIdentityAnalyzer>();
        context.TestCode = """
            namespace System.Linq
            {
                public static class Enumerable
                {
                    public static IEnumerable<T> DistinctBy<T>(this IEnumerable<T> items, Marker marker) => items;
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
                        return items.DistinctBy(marker);
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
