using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class SelectToIndexAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task Select_IndexFirstTuple_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1102:Select((x, i) => (i, x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexFirstTupleNamedElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int Index, string Item)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1102:Select((x, i) => (Index: i, Item: x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexFirstTupleStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ1102:Select(items, (x, i) => (i, x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTuple_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1102:Select((x, i) => (x, i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ1102:Select((x, i) => (x, i))|}.Select(t => t.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleWithSameTypeCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<object> items)
                {
                    return items.{|SLQ1102:Select((x, i) => ((string)x, i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleNullableSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string?, int)> MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ1102:Select((x, i) => (x, i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleNamedElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string Item, int Index)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1102:Select((x, i) => (Item: x, Index: i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleStatementBodyLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1102:Select((x, i) => { return (x, i); })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleGenericSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(T, int)> MyMethod<T>(IEnumerable<T> items)
                {
                    return items.{|SLQ1102:Select((x, i) => (x, i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ1102:Select(items, (x, i) => (x, i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleFullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ1102:Select(items, (x, i) => (x, i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleMultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (IEnumerable<(string, int)> Words, IEnumerable<(int, int)> Numbers) MyMethod(
                    IEnumerable<string> words, IEnumerable<int> numbers)
                {
                    var indexedWords = words.{|SLQ1102:Select((x, i) => (x, i))|};
                    var indexedNumbers = numbers.{|SLQ1102:Select((x, i) => (x, i))|};
                    return (indexedWords, indexedNumbers);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleWithDifferentNames_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string Value, int Count)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1102:Select((x, i) => (Value: x, Count: i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexFirstTupleWithDifferentNames_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int Value, string Element)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1102:Select((x, i) => (Value: i, Element: x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_DifferentElementSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, int)> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => (x.Length, i));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_DifferentIndexSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => (x, i + 1));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ThreeElementTuple_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int, int)> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => (x, i, x.Length));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_SingleArgumentSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Select(x => x);
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
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<string> items)
                {
                    return items.Select(Selector);
                }

                public static (string, int) Selector(string x, int i) => (x, i);
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_Index_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int Index, string Item)> MyMethod(IEnumerable<string> items)
                {
                    return items.Index();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_TargetFrameworkBeforeNet9_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net80);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => (x, i));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_TargetFrameworkNetStandard_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => (x, i));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_TargetFrameworkNet9_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net90);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1102:Select((x, i) => (x, i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
