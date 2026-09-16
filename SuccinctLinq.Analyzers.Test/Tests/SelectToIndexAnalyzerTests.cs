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
                    return items.{|SLQ202:Select((x, i) => (i, x))|};
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
                    return items.{|SLQ202:Select((x, i) => (Index: i, Item: x))|};
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
                    return Enumerable.{|SLQ202:Select(items, (x, i) => (i, x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetTuple_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => (i + 1, x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetTupleLiteralFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => (1 + i, x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetTupleSubtraction_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => (i - 1, x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetTupleZeroOffset_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => (i + 0, x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetTupleNamedElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => (Count: i + 1, Item: x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetTupleStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ202:Select(items, (x, i) => (i + 1, x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetTupleWithCast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<object> items)
                {
                    return items.Select((x, i) => (i + 1, (string)x));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexWideningCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(long, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => ((long)i, x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetTupleStatementBodyLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => { return (i + 1, x); })|};
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
                    return items.{|SLQ202:Select((x, i) => (x, i))|};
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
                    return items.Where(x => x.Length > 0).{|SLQ202:Select((x, i) => (x, i))|}.Select(t => t.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstTupleWithCast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(string, int)> MyMethod(IEnumerable<object> items)
                {
                    return items.Select((x, i) => ((string)x, i));
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
                    return items.{|SLQ202:Select((x, i) => (x, i))|};
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
                    return items.{|SLQ202:Select((x, i) => (Item: x, Index: i))|};
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
                    return items.{|SLQ202:Select((x, i) => { return (x, i); })|};
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
                    return items.{|SLQ202:Select((x, i) => (x, i))|};
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
                    return Enumerable.{|SLQ202:Select(items, (x, i) => (x, i))|};
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
                    return System.Linq.Enumerable.{|SLQ202:Select(items, (x, i) => (x, i))|};
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
                    var indexedWords = words.{|SLQ202:Select((x, i) => (x, i))|};
                    var indexedNumbers = numbers.{|SLQ202:Select((x, i) => (x, i))|};
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
                    return items.{|SLQ202:Select((x, i) => (Value: x, Count: i))|};
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
                    return items.{|SLQ202:Select((x, i) => (Value: i, Element: x))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_ElementFirstAnonymousObject_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => new { x, i })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexFirstAnonymousObject_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => new { i, x })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_AnonymousObjectWithNamedMembers_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => new { Value = x, Count = i })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_AnonymousObjectWithCast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<object> items)
                {
                    return items.Select((x, i) => new { Value = (string)x, i });
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetAnonymousObject_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ202:Select((x, i) => new { Offset = i + 1, x })|};
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
    public Task Select_ElementFirstOffsetTuple_NoWarning()
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
    public Task Select_IndexOffsetVariable_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items, int offset)
                {
                    return items.Select((x, i) => (i + offset, x));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetLongLiteral_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(long, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => (i + 1L, x));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetSubtractedIndex_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, string)> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => (1 - i, x));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_IndexOffsetTransformedElement_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<(int, int)> MyMethod(IEnumerable<int> items)
                {
                    return items.Select((x, i) => (i + 1, x + 1));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_AnonymousObjectExtraMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => new { x, i, x.Length });
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_AnonymousObjectSingleMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => new { x });
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_AnonymousObjectTransformedMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => new { x, Index = i + 1 });
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_AnonymousObjectMissingIndex_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<object> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => new { x, Length = x.Length });
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task Select_NonAnonymousObject_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<SelectToIndexAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<Pair> MyMethod(IEnumerable<string> items)
                {
                    return items.Select((x, i) => new Pair(x, i));
                }
            }

            public sealed record Pair(string X, int I);
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
                    return items.{|SLQ202:Select((x, i) => (x, i))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
