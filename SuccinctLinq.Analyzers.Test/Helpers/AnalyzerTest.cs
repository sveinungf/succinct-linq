using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace SuccinctLinq.Analyzers.Test.Helpers;

internal static class AnalyzerTest
{
    private const string ImplicitUsings = """
        global using System;
        global using System.Collections.Generic;
        global using System.IO;
        global using System.Linq;
        global using System.Net.Http;
        global using System.Threading;
        global using System.Threading.Tasks;
        """;

    public static CSharpAnalyzerTest<T, DefaultVerifier> CreateContext<T>(
        LanguageVersion? languageVersion = null,
        ReferenceAssemblies? referenceAssemblies = null)
        where T : DiagnosticAnalyzer, new()
    {
        var result = new CSharpAnalyzerTest<T, DefaultVerifier>
        {
            TestState =
            {
                ReferenceAssemblies = referenceAssemblies ?? ReferenceAssemblies.Net.Net100,
                Sources = { ("ImplicitUsings.g.cs", ImplicitUsings) },
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
