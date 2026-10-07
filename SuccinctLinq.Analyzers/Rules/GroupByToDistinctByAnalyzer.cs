using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GroupByToDistinctByAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ206",
        title: "GroupBy followed by Select of First can be simplified",
        messageFormat: "Use DistinctBy{0} instead",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Grouping elements by a key and taking the first element of each group keeps the first element of each distinct key, so DistinctBy expresses the same intent more concisely.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            // The DistinctBy() method is only available in .NET 6 and later.
            if (startContext.Compilation.IsTargetFrameworkAtLeast(6))
                startContext.RegisterOperationAction(Analyze, OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation { TargetMethod.IsAnySelectMethod: true } select)
            return;

        var preceding = select.GetArgumentAtOrDefault(0)?.UnwrapPreservingConversions();
        if (preceding is not IInvocationOperation { TargetMethod.IsGroupByMethod: true } groupBy)
            return;

        var selector = select.GetArgumentAtOrDefault(1);
        while (selector is IDelegateCreationOperation creation)
        {
            selector = creation.Target;
        }

        if (selector is not IAnonymousFunctionOperation lambda ||
            !lambda.IsFirstFunction())
        {
            return;
        }

        if (groupBy.Syntax is not InvocationExpressionSyntax groupByInvocation ||
            select.Syntax is not InvocationExpressionSyntax selectInvocation)
        {
            return;
        }

        var location = groupByInvocation.GetMethodChainLocation(selectInvocation);
        var hasComparer = groupBy.HasNonNullArgument(2);
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, hasComparer ? "(comparer)" : "()"));
    }
}
