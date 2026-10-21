using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class ProjectedGroupByToDictionaryFirstAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ210:GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndFirstInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static List<KeyValuePair<int, string>> MyMethod(IEnumerable<Item> items)
                {
                    return items.Where(x => x.Id > 0).{|SLQ210:GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First())|}.ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_WithComparer_ThenToDictionaryKeyAndFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ210:GroupBy(x => x.Id, x => x.Value, EqualityComparer<int>.Default).ToDictionary(g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_WithComparers_MessageMentionsComparers()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value, EqualityComparer<int>.Default).ToDictionary(g => g.Key, g => g.First(), EqualityComparer<int>.Default);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ210", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 14, 22, 14, 157)
                .WithMessage("Use DistinctBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(x => x.Id, x => x.Value, EqualityComparer<int>.Default) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_MessageOmitsComparers()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ210", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 14, 22, 14, 95)
                .WithMessage("Use DistinctBy(x => x.Id).ToDictionary(x => x.Id, x => x.Value) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ210:GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First(), EqualityComparer<int>.Default)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithIdentityElementSelector_ThenToDictionaryKeyAndFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
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
                    return items.{|SLQ210:GroupBy(x => x.Id, x => x).ToDictionary(g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithIdentityElementSelector_MessageOmitsElementSelector()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
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
                    return items.GroupBy(x => x.Id, x => x).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ210", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 89)
                .WithMessage("Use DistinctBy(x => x.Id).ToDictionary(x => x.Id) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithIdentityElementSelector_WithComparers_MessageOmitsElementSelector()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
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
                    return items.GroupBy(x => x.Id, x => x, EqualityComparer<int>.Default).ToDictionary(g => g.Key, g => g.First(), EqualityComparer<int>.Default);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ210", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 151)
                .WithMessage("Use DistinctBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(x => x.Id, EqualityComparer<int>.Default) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return Enumerable.{|SLQ210:GroupBy(items, x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_FullyQualifiedStaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return System.Linq.Enumerable.{|SLQ210:GroupBy(items, x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenStaticToDictionary_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return Enumerable.ToDictionary(items.{|SLQ210:GroupBy(x => x.Id, x => x.Value), g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_MultipleCalls_ReportWarningForEach()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static (Dictionary<int, string> ById, Dictionary<int, string> ByIdTwice) MyMethod(IEnumerable<Item> items)
                {
                    var byId = items.{|SLQ210:GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First())|};
                    var byIdTwice = items.{|SLQ210:GroupBy(x => x.Id * 2, x => x.Value).ToDictionary(g => g.Key, g => g.First())|};
                    return (byId, byIdTwice);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ210:GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.FirstOrDefault())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndLast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.Last());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_KeySelectorWithSideEffect_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    int n = 0;
                    return items.GroupBy(x => ++n, x => x.Value).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ElementSelectorWithSideEffect_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public int NextValue()
                {
                    return ++Id;
                }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.NextValue()).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndFirstWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First(y => y.Length > 0));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryOtherKeyAndFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key * 2, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndMemberOfFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First().Length);
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
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndFirstInAsyncLambda_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, Task<string>> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, async g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithoutElementSelector_ThenToDictionaryKeyAndFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
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
    public Task GroupBy_WithResultSelector_ThenToDictionaryKeyAndFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
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
    public Task ToDictionary_WithoutGroupBy_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<IGrouping<int, string>> groups)
                {
                    return groups.ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_TargetFrameworkBeforeNet6_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net50);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_TargetFrameworkNetStandard_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_TargetFrameworkNet6_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<ProjectedGroupByToDictionaryFirstAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net60);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }

                public string Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ210:GroupBy(x => x.Id, x => x.Value).ToDictionary(g => g.Key, g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
