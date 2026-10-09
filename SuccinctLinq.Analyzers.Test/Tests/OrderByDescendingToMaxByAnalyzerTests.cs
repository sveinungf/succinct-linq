using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class OrderByDescendingToMaxByAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task OrderByDescending_ThenFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_ThenFirstOrDefaultInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ205:OrderByDescending(x => x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithComparer_ThenFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => x.Length, Comparer<int>.Default).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithComparer_MessageMentionsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length, Comparer<int>.Default).FirstOrDefault();
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ205", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 22, 7, 94)
                .WithMessage("Use MaxBy(x => x.Length, Comparer<int>.Default) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_MessageOmitsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length).FirstOrDefault();
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ205", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 22, 7, 71)
                .WithMessage("Use MaxBy(x => x.Length) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithNullComparer_ThenFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => x.Length, null).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithNullComparerAndNullableKey_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => x, null).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithComparerAndReferenceTypeKey_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x, StringComparer.Ordinal).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithComparerAndNullableValueTypeKey_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => (int?)x.Length, Comparer<int?>.Default).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ205:OrderByDescending(items, x => x.Length).FirstOrDefault()|};
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
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ205:OrderByDescending(items, x => x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_ThenStaticFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.FirstOrDefault(items.{|SLQ205:OrderByDescending(x => x.Length))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithInterfaceElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public interface IMarker
            {
            }

            public static class MyClass
            {
                public static IMarker MyMethod(IEnumerable<IMarker> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => x.GetHashCode()).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithNullableReferenceElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static string? MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => x?.Length ?? 0).FirstOrDefault()|};
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
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (string Longest, string Shortest) MyMethod(IEnumerable<string> items)
                {
                    var longest = items.{|SLQ205:OrderByDescending(x => x.Length).FirstOrDefault()|};
                    var shortest = items.{|SLQ205:OrderByDescending(x => -x.Length).FirstOrDefault()|};
                    return (longest, shortest);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithValueTypeElements_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static int MyMethod(IEnumerable<int> items)
                {
                    return items.OrderByDescending(x => x).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithStructElements_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public struct Point
            {
                public int X;
            }

            public static class MyClass
            {
                public static Point MyMethod(IEnumerable<Point> items)
                {
                    return items.OrderByDescending(x => x.X).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithNullableValueTypeElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static int? MyMethod(IEnumerable<int?> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => x).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithUnconstrainedGenericElements_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static T MyMethod<T>(IEnumerable<T> items)
                {
                    return items.OrderByDescending(x => x.GetHashCode()).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithClassConstrainedGenericElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static T MyMethod<T>(IEnumerable<T> items)
                    where T : class
                {
                    return items.{|SLQ205:OrderByDescending(x => x.GetHashCode()).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithStructConstrainedGenericElements_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static T MyMethod<T>(IEnumerable<T> items)
                    where T : struct
                {
                    return items.OrderByDescending(x => x.GetHashCode()).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithReferenceTypeKey_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => x).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithNullableValueTypeKey_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => (int?)x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithGenericKey_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static TSource MyMethod<TSource, TKey>(IEnumerable<TSource> items, Func<TSource, TKey> keySelector)
                    where TSource : class
                {
                    return items.{|SLQ205:OrderByDescending(keySelector).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithStructConstrainedGenericKey_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static TSource MyMethod<TSource, TKey>(IEnumerable<TSource> items, Func<TSource, TKey> keySelector)
                    where TSource : class
                    where TKey : struct
                {
                    return items.{|SLQ205:OrderByDescending(keySelector).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task FirstOrDefault_WithoutOrderByDescending_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_WithoutFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
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
    public Task OrderByDescending_ThenFirstOrDefaultWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length).FirstOrDefault(x => x.Length > 0);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_ThenFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length).First();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_ThenSelect_ThenFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length).Select(x => x).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_ThenToList_ThenFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length).ToList().FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_ThenByDescending_ThenFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length).ThenByDescending(x => x).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_ThenFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_TargetFrameworkBeforeNet6_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net50);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length).FirstOrDefault();
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
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderByDescending(x => x.Length).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_TargetFrameworkNet6_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByDescendingToMaxByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net60);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ205:OrderByDescending(x => x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
