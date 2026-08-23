using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantDistinctAnalyzer : DiagnosticAnalyzer
{
    private const string EnumerableFullName = "System.Linq.Enumerable";

    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ1001",
        title: "Remove redundant Distinct() call",
        messageFormat: "Remove the redundant Distinct() call; ToHashSet() already removes duplicates",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Enumerable.ToHashSet() already removes duplicate elements, so calling Distinct() immediately before it is redundant.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(Analyze, OperationKind.Invocation);
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation toHashSet ||
            !IsLinqEnumerableMethod(toHashSet.TargetMethod, "ToHashSet"))
            return;

        var receiver = toHashSet.Instance ?? toHashSet.Arguments.FirstOrDefault()?.Value;
        if (receiver is not IInvocationOperation distinct ||
            !IsLinqEnumerableMethod(distinct.TargetMethod, "Distinct"))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, distinct.Syntax.GetLocation()));
    }

    private static bool IsLinqEnumerableMethod(IMethodSymbol method, string name)
        => string.Equals(method.Name, name, StringComparison.Ordinal)
            && method.Parameters.Length == 1
            && string.Equals(method.ContainingType?.ToDisplayString(), EnumerableFullName, StringComparison.Ordinal);
}
