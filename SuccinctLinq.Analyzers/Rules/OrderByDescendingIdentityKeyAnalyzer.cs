using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OrderByDescendingIdentityKeyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ203",
        title: "OrderByDescending can be simplified",
        messageFormat: "Use OrderDescending{0} instead",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An OrderByDescending with the identity function (x => x) is equivalent to the more concise OrderDescending.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            // The OrderDescending() method is only available in .NET 7 and later.
            if (startContext.Compilation.IsTargetFrameworkAtLeast(7))
                startContext.RegisterOperationAction(Analyze, OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation orderByDescending ||
            !orderByDescending.TargetMethod.IsOrderByDescendingMethod ||
            !orderByDescending.HasIdentitySelector(1, SymbolEqualityComparer.Default))
        {
            return;
        }

        if (orderByDescending.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var location = invocation.GetMethodCallLocation();
        var hasComparer = orderByDescending.TargetMethod.Parameters.Length > 2;
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, hasComparer ? "(comparer)" : "()"));
    }
}
