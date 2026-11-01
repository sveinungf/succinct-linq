using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class GroupByToDictionaryCountAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndCount_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ213:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndCountInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static List<KeyValuePair<int, int>> MyMethod(IEnumerable<Item> items)
                {
                    return items.Where(x => x.Id > 0).{|SLQ213:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count())|}.ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparer_ThenToDictionaryKeyAndCount_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ213:GroupBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(g => g.Key, g => g.Count())|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(g => g.Key, g => g.Count());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ213", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 112)
                .WithMessage("Use CountBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary() instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_MessageOmitsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ213", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 81)
                .WithMessage("Use CountBy(x => x.Id).ToDictionary() instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithNullComparer_MessageOmitsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, null).ToDictionary(g => g.Key, g => g.Count());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ213", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 87)
                .WithMessage("Use CountBy(x => x.Id).ToDictionary() instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ213:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count(), EqualityComparer<int>.Default)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryWithComparer_MessageMentionsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count(), EqualityComparer<int>.Default);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ213", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 112)
                .WithMessage("Use CountBy(x => x.Id).ToDictionary(EqualityComparer<int>.Default) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return Enumerable.{|SLQ213:GroupBy(items, x => x.Id).ToDictionary(g => g.Key, g => g.Count())|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return System.Linq.Enumerable.{|SLQ213:GroupBy(items, x => x.Id).ToDictionary(g => g.Key, g => g.Count())|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static (Dictionary<int, int> ById, Dictionary<int, int> ByIdTwice) MyMethod(IEnumerable<Item> items)
                {
                    var byId = items.{|SLQ213:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count())|};
                    var byIdTwice = items.{|SLQ213:GroupBy(x => x.Id * 2).ToDictionary(g => g.Key, g => g.Count())|};
                    return (byId, byIdTwice);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, Item> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndCountWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count(y => y.Id > 0));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryOtherKeyAndCount_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key * 2, g => g.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndCountPlusOne_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count() + 1);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndCount_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.ToString()).ToDictionary(g => g.Key, g => g.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithResultSelector_ThenToDictionaryKeyAndCount_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, (key, group) => (IGrouping<int, Item>)group).ToDictionary(g => g.Key, g => g.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryOfOtherExpression_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    var other = new List<Item> { new() };
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => other.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndCountInAsyncLambda_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, Task<int>> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, async g => g.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task ToDictionary_WithoutGroupBy_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<IGrouping<int, Item>> groups)
                {
                    return groups.ToDictionary(g => g.Key, g => g.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_TargetFrameworkBeforeNet9_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net80);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count());
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_TargetFrameworkNet9_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryCountAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net90);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ213:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Count())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
