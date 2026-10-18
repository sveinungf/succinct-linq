using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class GroupByToDictionaryToDistinctByAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.{|SLQ209:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static List<KeyValuePair<int, Item>> MyMethod(IEnumerable<Item> items)
                {
                    return items.Where(x => x.Id > 0).{|SLQ209:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First())|}.ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparer_ThenToDictionaryKeyAndFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.{|SLQ209:GroupBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(g => g.Key, g => g.First())|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ209", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 112)
                .WithMessage("Use DistinctBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(x => x.Id) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_MessageOmitsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ209", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 81)
                .WithMessage("Use DistinctBy(x => x.Id).ToDictionary(x => x.Id) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.{|SLQ209:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First(), EqualityComparer<int>.Default)|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return Enumerable.{|SLQ209:GroupBy(items, x => x.Id).ToDictionary(g => g.Key, g => g.First())|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return System.Linq.Enumerable.{|SLQ209:GroupBy(items, x => x.Id).ToDictionary(g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenStaticToDictionary_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return Enumerable.ToDictionary(items.{|SLQ209:GroupBy(x => x.Id), g => g.Key, g => g.First())|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static (Dictionary<int, Item> ById, Dictionary<int, Item> ByIdTwice) MyMethod(IEnumerable<Item> items)
                {
                    var byId = items.{|SLQ209:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First())|};
                    var byIdTwice = items.{|SLQ209:GroupBy(x => x.Id * 2).ToDictionary(g => g.Key, g => g.First())|};
                    return (byId, byIdTwice);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.{|SLQ209:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.FirstOrDefault())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndLast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Last());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_KeySelectorWithSideEffect_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    int n = 0;
                    return items.GroupBy(x => ++n).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First(y => y.Id > 0));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryOtherKeyAndFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key * 2, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndMemberOfFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Id);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.ToString()).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithResultSelector_ThenToDictionaryKeyAndFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id, (key, group) => (IGrouping<int, Item>)group).ToDictionary(g => g.Key, g => g.First());
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
                    var other = new List<Item> { new() };
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => other.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndCount_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
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
    public Task GroupBy_ThenToDictionaryKeyAndFirstInAsyncLambda_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, Task<Item>> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, async g => g.First());
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, Item> MyMethod(IEnumerable<IGrouping<int, Item>> groups)
                {
                    return groups.ToDictionary(g => g.Key, g => g.First());
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net50);
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
    public Task GroupBy_TargetFrameworkNetStandard_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
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
    public Task GroupBy_TargetFrameworkNet6_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryToDistinctByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net60);
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
                    return items.{|SLQ209:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
