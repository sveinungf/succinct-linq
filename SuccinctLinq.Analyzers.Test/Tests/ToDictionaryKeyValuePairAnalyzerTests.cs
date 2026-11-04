using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class ToDictionaryKeyValuePairAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task ToDictionary_KeyAndValueSelectors_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.{|SLQ104:ToDictionary(x => x.Key, x => x.Value)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_KeyAndValueSelectorsInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static List<KeyValuePair<int, string>> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.Where(x => x.Key > 0).{|SLQ104:ToDictionary(x => x.Key, x => x.Value)|}.ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_KeyAndValueSelectors_MessageSuggestsToDictionary()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.ToDictionary(x => x.Key, x => x.Value);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ104", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 22, 7, 60)
                .WithMessage("Use ToDictionary() instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return Enumerable.{|SLQ104:ToDictionary(pairs, x => x.Key, x => x.Value)|};
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return System.Linq.Enumerable.{|SLQ104:ToDictionary(pairs, x => x.Key, x => x.Value)|};
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.{|SLQ104:ToDictionary(elementSelector: x => x.Value, keySelector: x => x.Key)|};
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<TKey, TValue> MyMethod<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
                {
                    return pairs.{|SLQ104:ToDictionary(x => x.Key, x => x.Value)|};
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (Dictionary<int, string> First, Dictionary<string, int> Second) MyMethod(
                    IEnumerable<KeyValuePair<int, string>> first,
                    IEnumerable<KeyValuePair<string, int>> second)
                {
                    var dictionary1 = first.{|SLQ104:ToDictionary(x => x.Key, x => x.Value)|};
                    var dictionary2 = second.{|SLQ104:ToDictionary(x => x.Key, x => x.Value)|};
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.{|SLQ104:ToDictionary(x => { return x.Key; }, x => { return x.Value; })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NullForgivingValueSelectorOnNullableValueSource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string?>> pairs)
                {
                    return pairs.ToDictionary(x => x.Key, x => x.Value!);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NullForgivingKeySelectorOnNullableKeySource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<KeyValuePair<string?, int>> pairs)
                {
                    return pairs.ToDictionary(x => x.Key!, x => x.Value);
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string?> MyMethod(IEnumerable<KeyValuePair<int, string?>> pairs)
                {
                    return pairs.{|SLQ104:ToDictionary(x => x.Key, x => x.Value)|};
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<KeyValuePair<string, int>> pairs)
                {
                    return pairs.{|SLQ104:ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal)|};
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<KeyValuePair<string, int>> pairs)
                {
                    return pairs.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ104", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 22, 7, 84)
                .WithMessage("Use ToDictionary(StringComparer.Ordinal) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_WithNullComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.ToDictionary(x => x.Key, x => x.Value, null);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ104", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 22, 7, 66)
                .WithMessage("Use ToDictionary() instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_MethodGroupSelectors_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.ToDictionary(GetKey, GetValue);
                }

                private static int GetKey(KeyValuePair<int, string> pair) => pair.Key;

                private static string GetValue(KeyValuePair<int, string> pair) => pair.Value;
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_SwappedSelectors_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.ToDictionary(x => x.Value, x => x.Key);
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.ToDictionary(x => x.Key, x => x.Value.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_NonKeyValuePairSource_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
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
    public Task ToDictionary_Parameterless_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.ToDictionary();
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net70);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.ToDictionary(x => x.Key, x => x.Value);
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.ToDictionary(x => x.Key, x => x.Value);
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
        var context = AnalyzerTest.CreateContext<ToDictionaryKeyValuePairAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net80);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<KeyValuePair<int, string>> pairs)
                {
                    return pairs.{|SLQ104:ToDictionary(x => x.Key, x => x.Value)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
