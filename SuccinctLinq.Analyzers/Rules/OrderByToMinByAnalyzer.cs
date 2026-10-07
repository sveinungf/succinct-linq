using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OrderByToMinByAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ204",
        title: "OrderBy followed by FirstOrDefault can be simplified",
        messageFormat: "Use MinBy{0} instead",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ordering a sequence and taking the first element finds the element with the minimum key, so MinBy expresses the same intent more concisely. The rule only applies when the elements are nullable types and the key type is a non-nullable value type.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            // The MinBy() method is only available in .NET 6 and later.
            if (startContext.Compilation.IsTargetFrameworkAtLeast(6))
                startContext.RegisterOperationAction(Analyze, OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation { TargetMethod.IsFirstOrDefaultMethod: true } firstOrDefault)
            return;

        var preceding = firstOrDefault.GetArgument(0)?.UnwrapPreservingConversions();
        if (preceding is not IInvocationOperation { TargetMethod.IsOrderByMethod: true } orderBy)
            return;

        // MinBy is only equivalent for elements of nullable types:
        // On an empty sequence of non-nullable value types,
        // OrderBy(...).FirstOrDefault() returns the default value, while MinBy throws.
        if (!orderBy.TargetMethod.TypeArguments[0].IsNullableType)
            return;

        // MinBy is only equivalent for non-nullable keys:
        // OrderBy(...) orders null values first, while MinBy ignores null values.
        if (!orderBy.TargetMethod.TypeArguments[1].IsNonNullableValueType)
            return;

        if (orderBy.Syntax is not InvocationExpressionSyntax orderByInvocation ||
            firstOrDefault.Syntax is not InvocationExpressionSyntax firstOrDefaultInvocation)
        {
            return;
        }

        var location = orderByInvocation.GetMethodChainLocation(firstOrDefaultInvocation);
        var hasComparer = orderBy.HasNonNullArgument(2);
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, hasComparer ? "(comparer)" : "()"));
    }
}
