using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class ToDictionaryValueTupleAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task ToDictionary_Item1AndItem2Selectors_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.Item1, x => x.Item2)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_Item1AndItem2SelectorsInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static List<KeyValuePair<int, string>> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.Where(x => x.Item1 > 0).{|SLQ105:ToDictionary(x => x.Item1, x => x.Item2)|}.ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_Item1AndItem2Selectors_MessageSuggestsToDictionary()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1, x => x.Item2);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ105", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 23, 7, 63)
                .WithMessage("Simplify to just ToDictionary()"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return Enumerable.{|SLQ105:ToDictionary(tuples, x => x.Item1, x => x.Item2)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_FullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return System.Linq.Enumerable.{|SLQ105:ToDictionary(tuples, x => x.Item1, x => x.Item2)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NamedArgumentsOutOfOrder_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(elementSelector: x => x.Item2, keySelector: x => x.Item1)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NamedTupleSelectors_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int Key, string Value)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.Item1, x => x.Item2)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NamedTupleElementSelectors_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int Key, string Value)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.Key, x => x.Value)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_MixedPositionalAndNamedSelectors_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int Key, string Value)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.Item1, x => x.Value)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_ElementNamedItem_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int Item, string Name)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.Item, x => x.Name)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_SwappedNamedTupleElementSelectors_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<(int Key, string Value)> tuples)
                {
                    return tuples.ToDictionary(x => x.Value, x => x.Key);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_DuplicateTypeNamedTupleElementSelectors_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<(int A, int B)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.A, x => x.B)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_SwappedDuplicateTypeNamedTupleElementSelectors_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<(int A, int B)> tuples)
                {
                    return tuples.ToDictionary(x => x.B, x => x.A);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_GenericSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<TKey, TValue> MyMethod<TKey, TValue>(IEnumerable<(TKey, TValue)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.Item1, x => x.Item2)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_MultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (Dictionary<int, string> First, Dictionary<string, int> Second) MyMethod(
                    IEnumerable<(int, string)> first,
                    IEnumerable<(string, int)> second)
                {
                    var dictionary1 = first.{|SLQ105:ToDictionary(x => x.Item1, x => x.Item2)|};
                    var dictionary2 = second.{|SLQ105:ToDictionary(x => x.Item1, x => x.Item2)|};
                    return (dictionary1, dictionary2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_StatementBodySelectors_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => { return x.Item1; }, x => { return x.Item2; })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NullForgivingItem2SelectorOnNullableValueSource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string?)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1, x => x.Item2!);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NullForgivingItem1SelectorOnNullableKeySource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<(string?, int)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1!, x => x.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_IdentitySelectorsOnNullableSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string?> MyMethod(IEnumerable<(int, string?)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.Item1, x => x.Item2)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_ExplicitNonNullableElementOnNullableSource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string?)> tuples)
                {
                    return tuples.ToDictionary<(int, string?), int, string>(x => x.Item1, x => x.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_ExplicitNullableElementOnNonNullableSource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string?> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary<(int, string), int, string?>(x => x.Item1, x => x.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_ExplicitNullableNestedElementOnNonNullableSource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, IEnumerable<string?>> MyMethod(IEnumerable<(string, IEnumerable<string>)> tuples)
                {
                    return tuples.ToDictionary<(string, IEnumerable<string>), string, IEnumerable<string?>>(x => x.Item1, x => x.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_WithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<(string, int)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.Item1, x => x.Item2, StringComparer.Ordinal)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_WithComparer_MessageSuggestsToDictionaryWithComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<(string, int)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1, x => x.Item2, StringComparer.Ordinal);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ105", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 23, 7, 87)
                .WithMessage("Simplify to just ToDictionary(StringComparer.Ordinal)"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_WithNullComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1, x => x.Item2, null);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ105", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 23, 7, 69)
                .WithMessage("Simplify to just ToDictionary()"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_MethodGroupSelectors_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary(GetKey, GetValue);
                }

                private static int GetKey((int, string) tuple) => tuple.Item1;

                private static string GetValue((int, string) tuple) => tuple.Item2;
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_SwappedSelectors_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item2, x => x.Item1);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_DifferentElementSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1, x => x.Item2.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_ExplicitTypeArgumentsChangeValueType_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, object> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary<(int, string), int, object>(x => x.Item1, x => x.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_ExplicitNonNullableKeyOnNullableSource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<(string?, int)> tuples)
                {
                    return tuples.ToDictionary<(string?, int), string, int>(x => x.Item1, x => x.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NonValueTupleSource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Key { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.ToDictionary(x => x.Key, x => x.Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_HigherArityTuple_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string, bool)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1, x => x.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NestedTupleSource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<((int, string), bool)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1.Item1, x => x.Item1.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_Parameterless_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_TargetFrameworkBeforeNet8_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net70);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1, x => x.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_TargetFrameworkNetStandard_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.ToDictionary(x => x.Item1, x => x.Item2);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_TargetFrameworkNet8_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryValueTupleAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net80);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<(int, string)> tuples)
                {
                    return tuples.{|SLQ105:ToDictionary(x => x.Item1, x => x.Item2)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
