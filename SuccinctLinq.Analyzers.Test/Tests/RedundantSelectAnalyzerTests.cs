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
}
