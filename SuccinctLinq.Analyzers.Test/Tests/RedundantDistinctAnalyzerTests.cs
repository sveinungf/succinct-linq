using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class RedundantDistinctAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task RedundantDistinct_DistinctThenToHashSet_ReportWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            using System.Collections.Generic;
            using System.Linq;

            namespace MyNamespace;

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.{|SLQ1001:Distinct()|}.ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }

    [Fact]
    public Task RedundantDistinct_NonLinqEnumerable_NoWarning()
    {
        // Arrange
        var context = AnalyzerTest.CreateContext<RedundantDistinctAnalyzer>();
        context.TestCode = """
            using System.Collections.Generic;

            namespace MyNamespace;

            public static class Enumerable
            {
                public static IEnumerable<T> Distinct<T>(this IEnumerable<T> items) => items;
                public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items) => new HashSet<T>();
            }

            public static class MyClass
            {
                public static HashSet<string> MyMethod(IEnumerable<string> items)
                {
                    return items.Distinct().ToHashSet();
                }
            }
            """;

        // Act & Assert
        return context.RunAsync(Token);
    }
}
