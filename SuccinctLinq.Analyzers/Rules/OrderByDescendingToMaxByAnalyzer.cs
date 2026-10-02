using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OrderByDescendingToMaxByAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ205",
        title: "OrderByDescending followed by FirstOrDefault can be simplified",
        messageFormat: "Use MaxBy{0} instead",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ordering a sequence in descending order and taking the first element finds the element with the maximum key, so MaxBy expresses the same intent more concisely. The rule only applies when the elements are reference types.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            // The MaxBy() method is only available in .NET 6 and later.
            if (startContext.Compilation.IsTargetFrameworkAtLeast(6))
                startContext.RegisterOperationAction(Analyze, OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        // Compared to OrderByToMinByAnalyzer, we don't need the non-nullable key constraint,
        // because OrderByDescending orders null values last. If all elements are null, MaxBy will also return null.
        if (context.Operation is not IInvocationOperation firstOrDefault ||
            !firstOrDefault.TargetMethod.IsFirstOrDefaultMethod ||
            firstOrDefault.GetArgumentAtOrDefault(0)?.UnwrapPreservingConversions() is not IInvocationOperation orderByDescending ||
            !orderByDescending.TargetMethod.IsOrderByDescendingMethod ||
            // MaxBy is only equivalent for reference-type elements:
            // On an empty sequence of value types, OrderByDescending(...).FirstOrDefault() returns the default value, while MaxBy throws.
            !orderByDescending.TargetMethod.TypeArguments[0].IsReferenceType)
        {
            return;
        }

        if (orderByDescending.Syntax is not InvocationExpressionSyntax orderByDescendingInvocation ||
            firstOrDefault.Syntax is not InvocationExpressionSyntax firstOrDefaultInvocation)
        {
            return;
        }

        var location = orderByDescendingInvocation.GetMethodChainLocation(firstOrDefaultInvocation);
        var hasComparer = orderByDescending.TargetMethod.Parameters.Length > 2;
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, hasComparer ? "(comparer)" : "()"));
    }
}
