using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class RedundantSelectAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task Select_IdentitySelector_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ103:Select(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IdentitySelectorInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ103:Select(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ103:Select(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_StaticInvocationOutOfOrderNamedArguments_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ103:Select(selector: x => x, source: items)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_NamedSelector_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ103:Select(selector: x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_FullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ103:Select(items, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IdentitySelectorWithSameTypeCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ103:Select(x => (string)x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ExplicitReferenceDowncast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<object> items)
                {
                    return items.Select(x => (object)(string)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_BoxUnboxRoundTripCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<int> items)
                {
                    return items.{|SLQ103:Select(x => (int)(object)x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IdentitySelectorNullableSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string?> MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ103:Select(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_SelectorChangesNullability_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string?> items)
                {
                    return items.Select(x => x!);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ValueChangingCast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<double> MyMethod(IEnumerable<double> items)
                {
                    return items.Select(x => (double)(int)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_UserDefinedConversionCast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public struct Wrapper
            {
                public static explicit operator int(Wrapper wrapper) => 0;
                public static explicit operator Wrapper(int value) => default;
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<int> items)
                {
                    return items.Select(x => (int)(Wrapper)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_GenericSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<T> MyMethod<T>(IEnumerable<T> items)
                {
                    return items.{|SLQ103:Select(x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_MultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (IEnumerable<string> Words, IEnumerable<int> Numbers) MyMethod(
                    IEnumerable<string> words, IEnumerable<int> numbers)
                {
                    var selected = words.{|SLQ103:Select(x => x)|};
                    var other = numbers.{|SLQ103:Select(x => x)|};
                    return (selected, other);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_StatementBodyIdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ103:Select(x => { return x; })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_DifferentSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Select(x => x.Length.ToString());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_MethodGroupSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Select(Identity);
                }

                public static T Identity<T>(T value) => value;
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_WithIndexSelectorReturningElement_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ103:Select((x, i) => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexReturningSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<int> items)
                {
                    return items.Select((i, x) => x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_QuerySyntax_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return from x in items
                           select x;
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_QueryableSelect_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantSelectAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IQueryable<string> MyMethod(IQueryable<string> query)
                {
                    return query.Select(x => x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
