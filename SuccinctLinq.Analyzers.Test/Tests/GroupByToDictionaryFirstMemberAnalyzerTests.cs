using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class GroupByToDictionaryFirstMemberAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstMember_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ211:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Value)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstMemberInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static List<KeyValuePair<int, int>> MyMethod(IEnumerable<Item> items)
                {
                    return items.Where(x => x.Id > 0).{|SLQ211:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Value)|}.ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparer_ThenToDictionaryKeyAndFirstMember_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ211:GroupBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(g => g.Key, g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(g => g.Key, g => g.First().Value);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ211", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 13, 22, 13, 118)
                .WithMessage("Use DistinctBy(x => x.Id, EqualityComparer<int>.Default).ToDictionary(x => x.Id, g => g.Value) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_MessageOmitsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Value);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ211", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 13, 22, 13, 87)
                .WithMessage("Use DistinctBy(x => x.Id).ToDictionary(x => x.Id, g => g.Value) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_NestedMemberAccess_MessageShowsFullChain()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Address
            {
                public string City { get; set; }
            }

            public class Item
            {
                public int Id { get; set; }
                public Address Address { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Address.City);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ211", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 18, 22, 18, 94)
                .WithMessage("Use DistinctBy(x => x.Id).ToDictionary(x => x.Id, g => g.Address.City) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstMemberWithEscapedParameter_MessagePreservesEscape()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, @class => @class.First().Value);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ211", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 13, 22, 13, 97)
                .WithMessage("Use DistinctBy(x => x.Id).ToDictionary(x => x.Id, @class => @class.Value) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithNullComparer_ThenToDictionaryKeyAndFirstMember_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ211:GroupBy(x => x.Id, null).ToDictionary(g => g.Key, g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return Enumerable.{|SLQ211:GroupBy(items, x => x.Id).ToDictionary(g => g.Key, g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return System.Linq.Enumerable.{|SLQ211:GroupBy(items, x => x.Id).ToDictionary(g => g.Key, g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return Enumerable.ToDictionary(items.{|SLQ211:GroupBy(x => x.Id), g => g.Key, g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static (Dictionary<int, int> ById, Dictionary<int, int> ByIdTwice) MyMethod(IEnumerable<Item> items)
                {
                    var byId = items.{|SLQ211:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Value)|};
                    var byIdTwice = items.{|SLQ211:GroupBy(x => x.Id * 2).ToDictionary(g => g.Key, g => g.First().Value)|};
                    return (byId, byIdTwice);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstOrDefaultMember_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ211:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.FirstOrDefault().Value)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryWithComparer_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ211:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Value, EqualityComparer<int>.Default)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstMemberWithTrailingNullForgiving_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public string? Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Value!);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ211", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 13, 22, 13, 88)
                .WithMessage("Use DistinctBy(x => x.Id).ToDictionary(x => x.Id, g => g.Value!) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_TargetFrameworkNet6_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net60);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ211:GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
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
    public Task GroupBy_ThenToDictionaryKeyAndLastMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Last().Value);
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    int n = 0;
                    return items.GroupBy(x => ++n).ToDictionary(g => g.Key, g => g.First().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstMemberWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First(y => y.Id > 0).Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryOtherKeyAndFirstMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key * 2, g => g.First().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenToDictionaryKeyAndFirstMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public string Tag { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Tag).ToDictionary(g => g.Key, g => g.First().ToString());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithResultSelector_ThenToDictionaryKeyAndFirstMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, (key, group) => (IGrouping<int, Item>)group).ToDictionary(g => g.Key, g => g.First().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryOfOtherExpressionFirstMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    var other = new List<Item> { new() };
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => other.First().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstMemberWithNullForgivingAfterFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First()!.Value);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ211", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 13, 22, 13, 88)
                .WithMessage("Use DistinctBy(x => x.Id).ToDictionary(x => x.Id, g => g!.Value) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstMemberWithNullForgivingInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Address
            {
                public string City { get; set; }
            }

            public class Item
            {
                public int Id { get; set; }
                public Address? Address { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Address!.City);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ211", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 18, 22, 18, 95)
                .WithMessage("Use DistinctBy(x => x.Id).ToDictionary(x => x.Id, g => g.Address!.City) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstMemberIndexer_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int this[int index] { get { return index; } set { } }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First()[0]);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenToDictionaryKeyAndFirstMemberWithIndexerInChain_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Holder
            {
                public string this[int index] { get { return string.Empty; } set { } }
            }

            public class Item
            {
                public int Id { get; set; }
                public Holder Holder { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Holder[0]);
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
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
    public Task GroupBy_ThenToDictionaryKeyAndFirstMemberAsyncLambda_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, Task<int>> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, async g => g.First().Value);
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<IGrouping<int, Item>> groups)
                {
                    return groups.ToDictionary(g => g.Key, g => g.First().Value);
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net50);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Value);
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
        var context = AnalyzerTest.CreateContext<GroupByToDictionaryFirstMemberAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static Dictionary<int, int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
