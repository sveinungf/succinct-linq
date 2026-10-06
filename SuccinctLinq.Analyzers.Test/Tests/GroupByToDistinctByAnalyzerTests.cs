using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class GroupByToDistinctByAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task GroupBy_ThenSelectFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ206:GroupBy(x => x.Id).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static List<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.Where(x => x.Id > 0).{|SLQ206:GroupBy(x => x.Id).Select(g => g.First())|}.ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparer_ThenSelectFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ206:GroupBy(x => x.Id, EqualityComparer<int>.Default).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparer_MessageMentionsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, EqualityComparer<int>.Default).Select(g => g.First());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ206", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 94)
                .WithMessage("Use DistinctBy(comparer) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_MessageOmitsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ206", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 63)
                .WithMessage("Use DistinctBy() instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithNullComparer_ThenSelectFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ206:GroupBy(x => x.Id, null).Select(g => g.First())|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return Enumerable.{|SLQ206:GroupBy(items, x => x.Id).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_FullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return System.Linq.Enumerable.{|SLQ206:GroupBy(items, x => x.Id).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenStaticSelect_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return Enumerable.Select(items.{|SLQ206:GroupBy(x => x.Id), g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstWithIndex_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ206:GroupBy(x => x.Id).Select((g, i) => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_MultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static (IEnumerable<Item> ById, IEnumerable<Item> ByIdTwice) MyMethod(IEnumerable<Item> items)
                {
                    var byId = items.{|SLQ206:GroupBy(x => x.Id).Select(g => g.First())|};
                    var byIdTwice = items.{|SLQ206:GroupBy(x => x.Id * 2).Select(g => g.First())|};
                    return (byId, byIdTwice);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.ToString()).Select(g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First(y => y.Id > 0));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ206:GroupBy(x => x.Id).Select(g => g.FirstOrDefault())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparer_ThenSelectFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ206:GroupBy(x => x.Id, EqualityComparer<int>.Default).Select(g => g.FirstOrDefault())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstOrDefaultWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.FirstOrDefault(y => y.Id > 0));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectLast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.Last());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectCount_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstOfOtherExpression_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    var other = new List<Item> { new() };
                    return items.GroupBy(x => x.Id).Select(g => other.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task SelectFirst_WithoutGroupBy_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<IGrouping<int, Item>> groups)
                {
                    return groups.Select(g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithoutSelect_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, Item>> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_TargetFrameworkBeforeNet6_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net50);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_TargetFrameworkNetStandard_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_TargetFrameworkNet6_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDistinctByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net60);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ206:GroupBy(x => x.Id).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
