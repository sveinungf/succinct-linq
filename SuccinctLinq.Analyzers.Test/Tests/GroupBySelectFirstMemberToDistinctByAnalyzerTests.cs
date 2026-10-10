using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class GroupBySelectFirstMemberToDistinctByAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task GroupBy_ThenSelectFirstMember_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ208:GroupBy(x => x.Id).Select(g => g.First().Value)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstMemberInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static List<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.Where(x => x.Id > 0).{|SLQ208:GroupBy(x => x.Id).Select(g => g.First().Value)|}.ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparer_ThenSelectFirstMember_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ208:GroupBy(x => x.Id, EqualityComparer<int>.Default).Select(g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, EqualityComparer<int>.Default).Select(g => g.First().Value);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ208", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 13, 22, 13, 100)
                .WithMessage("Use DistinctBy(x => x.Id, EqualityComparer<int>.Default).Select(g => g.Value) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_MessageOmitsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First().Value);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ208", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 13, 22, 13, 69)
                .WithMessage("Use DistinctBy(x => x.Id).Select(g => g.Value) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_NestedMemberAccess_MessageShowsFullChain()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
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
                public static IEnumerable<string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First().Address.City);
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ208", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 18, 22, 18, 76)
                .WithMessage("Use DistinctBy(x => x.Id).Select(g => g.Address.City) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithNullComparer_ThenSelectFirstMember_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ208:GroupBy(x => x.Id, null).Select(g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return Enumerable.{|SLQ208:GroupBy(items, x => x.Id).Select(g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return System.Linq.Enumerable.{|SLQ208:GroupBy(items, x => x.Id).Select(g => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return Enumerable.Select(items.{|SLQ208:GroupBy(x => x.Id), g => g.First().Value)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstMemberWithIndex_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ208:GroupBy(x => x.Id).Select((g, i) => g.First().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static (IEnumerable<int> ById, IEnumerable<int> ByIdTwice) MyMethod(IEnumerable<Item> items)
                {
                    var byId = items.{|SLQ208:GroupBy(x => x.Id).Select(g => g.First().Value)|};
                    var byIdTwice = items.{|SLQ208:GroupBy(x => x.Id * 2).Select(g => g.First().Value)|};
                    return (byId, byIdTwice);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstOrDefaultMember_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ208:GroupBy(x => x.Id).Select(g => g.FirstOrDefault().Value)|};
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
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>(
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
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.{|SLQ208:GroupBy(x => x.Id).Select(g => g.First().Value)|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
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
    public Task GroupBy_WithElementSelector_ThenSelectFirstMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public string Tag { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.Tag).Select(g => g.First().ToString());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithResultSelector_ThenSelectFirstMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, (key, group) => group.Reverse()).Select(g => g.First().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstMemberWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First(y => y.Id > 0).Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstOrDefaultMemberWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.FirstOrDefault(y => y.Id > 0).Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectLastMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.Last().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstOfOtherExpressionMember_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    var other = new List<Item> { new() };
                    return items.GroupBy(x => x.Id).Select(g => other.First().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstMemberWithDowncast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public interface IGroup : IGrouping<int, Item>
            {
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => ((IGroup)g).First().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstMemberIndexer_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int this[int index] { get { return index; } set { } }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First()[0]);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_ThenSelectFirstMemberWithIndexerInChain_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
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
                public static IEnumerable<string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First().Holder[0]);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task SelectFirstMember_WithoutGroupBy_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<int> MyMethod(IEnumerable<IGrouping<int, Item>> groups)
                {
                    return groups.Select(g => g.First().Value);
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
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
                public int Value { get; set; }
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
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>(
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
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First().Value);
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
        var context = AnalyzerTest.CreateContext<GroupBySelectFirstMemberToDistinctByAnalyzer>(
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
                public static IEnumerable<int> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id).Select(g => g.First().Value);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
