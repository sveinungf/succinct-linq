using SuccinctLinq.Analyzers.Rules;
using SuccinctLinq.Analyzers.Test.Helpers;

namespace SuccinctLinq.Analyzers.Test.Tests;

public class RedundantDistinctAnalyzerTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public Task RedundantDistinct_DistinctThenToHashSet()
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
}
