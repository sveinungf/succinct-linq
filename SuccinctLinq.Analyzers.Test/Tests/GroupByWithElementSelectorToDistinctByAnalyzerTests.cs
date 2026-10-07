using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class GroupByWithElementSelectorToDistinctByAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.{|SLQ207:GroupBy(x => x.Id, x => x.ToString()).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectFirstInChain_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static List<string> MyMethod(IEnumerable<Item> items)
                {
                    return items.Where(x => x.Id > 0).{|SLQ207:GroupBy(x => x.Id, x => x.ToString()).Select(g => g.First())|}.ToList();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparerAndElementSelector_ThenSelectFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.{|SLQ207:GroupBy(x => x.Id, x => x.ToString(), EqualityComparer<int>.Default).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparerAndElementSelector_MessageShowsExactComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id, x => x.ToString(), EqualityComparer<int>.Default).Select(g => g.First());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ207", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 113)
                .WithMessage("Use DistinctBy(EqualityComparer<int>.Default).Select(x => x.ToString()) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparerVariableAndElementSelector_MessageShowsComparerVariable()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<Item> items, IEqualityComparer<int> myComparer)
                {
                    return items.GroupBy(x => x.Id, x => x.ToString(), myComparer).Select(g => g.First());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ207", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 94)
                .WithMessage("Use DistinctBy(myComparer).Select(x => x.ToString()) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparerOnNextLineAndElementSelector_MessageShowsExactComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<Item> items, IEqualityComparer<int> c)
                {
                    return items.GroupBy(x => x.Id, x => x.ToString(),
                        c).Select(g => g.First());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ207", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 13, 38)
                .WithMessage("Use DistinctBy(c).Select(x => x.ToString()) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithComparerContainingStringLiteral_MessageKeepsSyntaxUntouched()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public sealed class NamedComparer : IEqualityComparer<int>
            {
                public NamedComparer(string _)
                {
                }

                public bool Equals(int x, int y) => x == y;

                public int GetHashCode(int obj) => obj.GetHashCode();
            }

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.ToString(), new NamedComparer("a b")).Select(g => g.First());
                }
            }
            """;
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ207", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 23, 22, 23, 108)
                .WithMessage("Use DistinctBy(new NamedComparer(\"a b\")).Select(x => x.ToString()) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_MessageOmitsComparer()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
        context.TestState.ExpectedDiagnostics.Add(
            new DiagnosticResult("SLQ207", DiagnosticSeverity.Warning)
                .WithSpan("/0/Test1.cs", 12, 22, 12, 82)
                .WithMessage("Use DistinctBy().Select(x => x.ToString()) instead"));

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithNullComparerAndElementSelector_ThenSelectFirst_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.{|SLQ207:GroupBy(x => x.Id, x => x.ToString(), null).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_StaticInvocation_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return Enumerable.{|SLQ207:GroupBy(items, x => x.Id, x => x.ToString()).Select(g => g.First())|};
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
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return System.Linq.Enumerable.{|SLQ207:GroupBy(items, x => x.Id, x => x.ToString()).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenStaticSelect_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return Enumerable.Select(items.{|SLQ207:GroupBy(x => x.Id, x => x.ToString()), g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectFirstWithIndex_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.{|SLQ207:GroupBy(x => x.Id, x => x.ToString()).Select((g, i) => g.First())|};
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
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static (IEnumerable<string> ById, IEnumerable<string> ByIdTwice) MyMethod(IEnumerable<Item> items)
                {
                    var byId = items.{|SLQ207:GroupBy(x => x.Id, x => x.ToString()).Select(g => g.First())|};
                    var byIdTwice = items.{|SLQ207:GroupBy(x => x.Id * 2, x => x.ToString()).Select(g => g.First())|};
                    return (byId, byIdTwice);
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectFirstOrDefault_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.{|SLQ207:GroupBy(x => x.Id, x => x.ToString()).Select(g => g.FirstOrDefault())|};
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
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net60);
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
                    return items.{|SLQ207:GroupBy(x => x.Id, x => x.ToString()).Select(g => g.First())|};
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithoutElementSelector_ThenSelectFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
    public Task GroupBy_WithResultSelector_ThenSelectFirst_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id, (key, group) => group.Reverse()).Select(g => g.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectFirstWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id, x => x.ToString()).Select(g => g.First(y => y.Length > 0));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectFirstOrDefaultWithPredicate_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id, x => x.ToString()).Select(g => g.FirstOrDefault(y => y.Length > 0));
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectLast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id, x => x.ToString()).Select(g => g.Last());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectCount_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    return items.GroupBy(x => x.Id, x => x.ToString()).Select(g => g.Count());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectFirstOfOtherExpression_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
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
                    var other = new List<string> { "a" };
                    return items.GroupBy(x => x.Id, x => x.ToString()).Select(g => other.First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_ThenSelectFirstWithDowncast_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public interface IGroup : IGrouping<int, string>
            {
            }

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.ToString()).Select(g => ((IGroup)g).First());
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task GroupBy_WithElementSelector_WithoutSelect_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<IGrouping<int, string>> MyMethod(IEnumerable<Item> items)
                {
                    return items.GroupBy(x => x.Id, x => x.ToString());
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
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>();
        context.TestCode = """
            namespace MyNamespace;

            public class Item
            {
                public int Id { get; set; }
            }

            public static class MyClass
            {
                public static IEnumerable<string> MyMethod(IEnumerable<IGrouping<int, string>> groups)
                {
                    return groups.Select(g => g.First());
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
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.Net.Net50);
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
    public Task GroupBy_WithElementSelector_TargetFrameworkNetStandard_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<GroupByWithElementSelectorToDistinctByAnalyzer>(
            referenceAssemblies: ReferenceAssemblies.NetStandard.NetStandard20);
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
}
