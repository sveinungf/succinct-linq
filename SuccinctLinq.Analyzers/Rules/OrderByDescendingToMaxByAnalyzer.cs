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
        description: "Ordering a sequence in descending order and taking the first element finds the element with the maximum key, so MaxBy expresses the same intent more concisely. The rule only applies when the elements are nullable types and, when a custom comparer is used, the key type is a non-nullable value type.");

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
        // Compared to OrderByToMinByAnalyzer, we don't need the non-nullable key constraint
        // when no custom comparer is used, because OrderByDescending orders null keys last
        // while MaxBy ignores null keys, so both select the same element.
        if (context.Operation is not IInvocationOperation firstOrDefault ||
            !firstOrDefault.TargetMethod.IsFirstOrDefaultMethod ||
            firstOrDefault.GetArgumentAtOrDefault(0)?.UnwrapPreservingConversions() is not IInvocationOperation orderByDescending ||
            !orderByDescending.TargetMethod.IsOrderByDescendingMethod ||
            // MaxBy is only equivalent for elements of nullable types:
            // On an empty sequence of non-nullable value types,
            // OrderByDescending(...).FirstOrDefault() returns the default value, while MaxBy throws.
            !orderByDescending.TargetMethod.TypeArguments[0].IsNullableType ||
            // A custom comparer may rank null keys above non-null keys:
            // OrderByDescending(..., comparer) then selects the null-key element,
            // while MaxBy(..., comparer) ignores null keys.
            orderByDescending.HasNonNullArgument(2) && orderByDescending.TargetMethod.TypeArguments[1].IsNullableType)
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
