using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace SuccinctLinq.Analyzers.Test.Helpers;

internal static class AnalyzerTest
{
    public static CSharpAnalyzerTest<T, DefaultVerifier> CreateContext<T>(
        LanguageVersion? languageVersion = null)
        where T : DiagnosticAnalyzer, new()
    {
        var result = new CSharpAnalyzerTest<T, DefaultVerifier>
        {
            TestState =
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net100,
                AdditionalReferences =
                {
                    MetadataReference.CreateFromFile(typeof(T).Assembly.Location)
                }
            }
        };

        if (languageVersion is { } version)
        {
            result.SolutionTransforms.Add((solution, projectId) =>
            {
                var newOptions = new CSharpParseOptions(version);
                return solution.WithProjectParseOptions(projectId, newOptions);
            });
        }

        return result;
    }
}
