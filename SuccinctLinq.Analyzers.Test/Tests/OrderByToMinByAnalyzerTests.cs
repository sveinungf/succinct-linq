using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class OrderByToMinByAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task OrderBy_ThenFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ204:OrderBy(x => x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_ThenFirstOrDefaultInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.Where(x => x.Length > 0).{|SLQ204:OrderBy(x => x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithComparer_ThenFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ204:OrderBy(x => x.Length, Comparer<int>.Default).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithComparer_MessageMentionsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length, Comparer<int>.Default).FirstOrDefault();
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ204", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 22, 7, 84)
                .WithMessage("Use MinBy(comparer) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_MessageOmitsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
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
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ204", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 7, 22, 7, 61)
                .WithMessage("Use MinBy() instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithNullComparer_ThenFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ204:OrderBy(x => x.Length, null).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.{|SLQ204:OrderBy(items, x => x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_FullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return System.Linq.Enumerable.{|SLQ204:OrderBy(items, x => x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_ThenStaticFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return Enumerable.FirstOrDefault(items.{|SLQ204:OrderBy(x => x.Length))|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithInterfaceElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public interface IMarker
            {
            }

            public static class MyClass
            {
                public static IMarker MyMethod(IEnumerable<IMarker> items)
                {
                    return items.{|SLQ204:OrderBy(x => x.GetHashCode()).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithNullableReferenceElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            #nullable enable

            namespace MyNamespace;

            public static class MyClass
            {
                public static string? MyMethod(IEnumerable<string?> items)
                {
                    return items.{|SLQ204:OrderBy(x => x?.Length ?? 0).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_MultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static (string Shortest, string Longest) MyMethod(IEnumerable<string> items)
                {
                    var shortest = items.{|SLQ204:OrderBy(x => x.Length).FirstOrDefault()|};
                    var longest = items.{|SLQ204:OrderBy(x => -x.Length).FirstOrDefault()|};
                    return (shortest, longest);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithValueTypeElements_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static int MyMethod(IEnumerable<int> items)
                {
                    return items.OrderBy(x => x).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithStructElements_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
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
                    return items.OrderBy(x => x.X).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithUnconstrainedGenericElements_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static T MyMethod<T>(IEnumerable<T> items)
                {
                    return items.OrderBy(x => x.GetHashCode()).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithClassConstrainedGenericElements_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static T MyMethod<T>(IEnumerable<T> items)
                    where T : class
                {
                    return items.{|SLQ204:OrderBy(x => x.GetHashCode()).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithStructConstrainedGenericElements_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static T MyMethod<T>(IEnumerable<T> items)
                    where T : struct
                {
                    return items.OrderBy(x => x.GetHashCode()).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithReferenceTypeKey_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithNullableValueTypeKey_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => (int?)x.Length).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithGenericKey_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static TSource MyMethod<TSource, TKey>(IEnumerable<TSource> items, Func<TSource, TKey> keySelector)
                    where TSource : class
                {
                    return items.OrderBy(keySelector).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_WithStructConstrainedGenericKey_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static TSource MyMethod<TSource, TKey>(IEnumerable<TSource> items, Func<TSource, TKey> keySelector)
                    where TSource : class
                    where TKey : struct
                {
                    return items.OrderBy(keySelector).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task FirstOrDefault_WithoutOrderBy_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
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
    public Task OrderBy_WithoutFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static IOrderedEnumerable<string> MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_ThenFirstOrDefaultWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length).FirstOrDefault(x => x.Length > 0);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_ThenFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length).First();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_ThenSelect_ThenFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length).Select(x => x).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_ThenToList_ThenFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length).ToList().FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderBy_ThenBy_ThenFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.OrderBy(x => x.Length).ThenBy(x => x).FirstOrDefault();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task OrderByDescending_ThenFirstOrDefault_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>();
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
    public Task OrderBy_TargetFrameworkBeforeNet6_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net50);
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
    public Task OrderBy_TargetFrameworkNetStandard_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
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
    public Task OrderBy_TargetFrameworkNet6_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<OrderByToMinByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net60);
        context.TestCode = """
            namespace MyNamespace;

            public static class MyClass
            {
                public static string MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ204:OrderBy(x => x.Length).FirstOrDefault()|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
