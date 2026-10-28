using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DistinctByIdentityAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ212",
        title: "DistinctBy can be simplified",
        messageFormat: "Use Distinct({0}) instead",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A DistinctBy with the identity function (x => x) is equivalent to Distinct.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(Analyze, OperationKind.Invocation);
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation distinctBy ||
            !distinctBy.TargetMethod.IsDistinctByMethod ||
            !distinctBy.HasIdentitySelector(1, SymbolEqualityComparer.Default))
        {
            return;
        }

        if (distinctBy.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var location = invocation.GetMethodCallLocation();

        // The comparer argument is optional.
        var comparerArg = distinctBy.GetArgument(2)?.UnwrapConversions();
        var comparerText = comparerArg is { IsNullOrDefault: false }
            ? comparerArg.GetSingleLineSyntaxText() ?? "{comparer}"
            : "";

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, comparerText));
    }
}
