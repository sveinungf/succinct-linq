using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class RedundantElementSelectorAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task ToDictionary_IdentityElementSelector_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToDictionary(x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_IdentityElementSelectorInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ102:ToDictionary(x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_IdentityElementSelectorWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToDictionary(x => x.Length, x => x, EqualityComparer<int>.Default)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ102:ToDictionary(items, x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_StaticInvocationOutOfOrderNamedArguments_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ102:ToDictionary(keySelector: x => x.Length, elementSelector: x => x, source: items)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_OutOfOrderNamedArguments_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToDictionary(elementSelector: x => x, keySelector: x => x.Length)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_OutOfOrderNamedArgumentsWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToDictionary(keySelector: x => x.Length, comparer: EqualityComparer<int>.Default, elementSelector: x => x)|};
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
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ102:ToDictionary(items, x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_IdentityElementSelectorWithSameTypeCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToDictionary(x => x.Length, x => (string)x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_ValueChangingCast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<double, double> MyMethod(IEnumerable<double> items)
                {
                    return items.ToDictionary(x => x, x => (double)(int)x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_IdentityElementSelectorNullableSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string?> MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ102:ToDictionary(x => x!.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_ElementSelectorChangesNullability_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string?> items)
                {
                    return items.ToDictionary(x => x!.Length, x => x!);
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
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, T> MyMethod<T>(IEnumerable<T> items)
                {
                    return items.{|SLQ102:ToDictionary(x => x.ToString(), x => x)|};
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
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (Dictionary<int, string> Words, Dictionary<string, int> Numbers) MyMethod(
                    IEnumerable<string> words, IEnumerable<int> numbers)
                {
                    var dictionary = words.{|SLQ102:ToDictionary(x => x.Length, x => x)|};
                    var other = numbers.{|SLQ102:ToDictionary(x => x.ToString(), x => x)|};
                    return (dictionary, other);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_StatementBodyIdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToDictionary(x => x.Length, x => { return x; })|};
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
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.ToDictionary(x => x.Length, x => x.Length.ToString());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_DifferentElementType_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, int> MyMethod(IEnumerable<string> items)
                {
                    return items.ToDictionary(x => x, x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_WithoutElementSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.ToDictionary(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_IdentityKeySelectorWithoutElementSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<string, string> MyMethod(IEnumerable<string> items)
                {
                    return items.ToDictionary(x => x);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_MethodGroupElementSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.ToDictionary(x => x.Length, Identity);
                }

                public static T Identity<T>(T value) => value;
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_WithNonElementSelectorParameter_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace System.Linq
            {
                public static class Enumerable
                {
                    public static IEnumerable<T> ToDictionary<T>(
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
                        return items.ToDictionary(x => x, marker);
                    }
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_IdentityElementSelector_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToLookup(x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_IdentityElementSelectorInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ102:ToLookup(x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_IdentityElementSelectorWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToLookup(x => x.Length, x => x, EqualityComparer<int>.Default)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ102:ToLookup(items, x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_OutOfOrderNamedArguments_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToLookup(elementSelector: x => x, keySelector: x => x.Length)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_IdentityElementSelectorWithSameTypeCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToLookup(x => x.Length, x => (string)x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_IdentityElementSelectorNullableSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string?> MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ102:ToLookup(x => x!.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_ElementSelectorChangesNullability_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string?> items)
                {
                    return items.ToLookup(x => x!.Length, x => x!);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_StatementBodyIdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:ToLookup(x => x.Length, x => { return x; })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_DifferentElementSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.ToLookup(x => x.Length, x => x.Length.ToString());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_DifferentElementType_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<string, int> MyMethod(IEnumerable<string> items)
                {
                    return items.ToLookup(x => x, x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToLookup_WithoutElementSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static ILookup<int, string> MyMethod(IEnumerable<string> items)
                {
                    return items.ToLookup(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_IdentityElementSelector_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:GroupBy(x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_IdentityElementSelectorInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ102:GroupBy(x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_IdentityElementSelectorWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:GroupBy(x => x.Length, x => x, EqualityComparer<int>.Default)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ102:GroupBy(items, x => x.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_OutOfOrderNamedArguments_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:GroupBy(elementSelector: x => x, keySelector: x => x.Length)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_IdentityElementSelectorWithSameTypeCast_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:GroupBy(x => x.Length, x => (string)x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_IdentityElementSelectorNullableSource_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string?>> MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ102:GroupBy(x => x!.Length, x => x)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ElementSelectorChangesNullability_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string?> items)
                {
                    return items.GroupBy(x => x!.Length, x => x!);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_StatementBodyIdentityLambda_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ102:GroupBy(x => x.Length, x => { return x; })|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_DifferentElementSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string> items)
                {
                    return items.GroupBy(x => x.Length, x => x.Length.ToString());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_DifferentElementType_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<string, int>> MyMethod(IEnumerable<string> items)
                {
                    return items.GroupBy(x => x, x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithoutElementSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<string> items)
                {
                    return items.GroupBy(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithResultSelector_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantElementSelectorAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<string> items)
                {
                    return items.GroupBy(x => x.Length, x => x, (length, group) => group.Count(), EqualityComparer<int>.Default);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
