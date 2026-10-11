using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GroupBySelectFirstMemberToDistinctByAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ208",
        title: "GroupBy followed by Select of a member of First can be simplified",
        messageFormat: "Use DistinctBy({0}{1}).Select({2}) instead",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Grouping elements by a key and taking a member of the first element of each group is equivalent to keeping the first element of each distinct key and selecting the member, so DistinctBy followed by Select expresses the same intent more concisely.");

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

        var preceding = select.GetArgument(0)?.UnwrapPreservingConversions();
        if (preceding is not IInvocationOperation { TargetMethod.IsGroupByMethod: true } groupBy)
            return;

        var selector = select.GetArgument(1);
        while (selector is IDelegateCreationOperation creation)
        {
            selector = creation.Target;
        }

        if (selector is not IAnonymousFunctionOperation lambda ||
            !lambda.TryGetMemberNamesOnFirstElement(out var memberNames, out var trailingNullForgiving))
        {
            return;
        }

        if (groupBy.Syntax is not InvocationExpressionSyntax groupByInvocation ||
            select.Syntax is not InvocationExpressionSyntax selectInvocation)
        {
            return;
        }

        var location = groupByInvocation.GetMethodChainLocation(selectInvocation);

        var keySelectorText = groupBy.GetArgument(1)?.GetSingleLineSyntaxText() ?? "{keySelector}";

        // The comparer argument is optional.
        var comparerText = groupBy.GetArgument(2) is { } comparerArg
            ? $", {comparerArg.GetSingleLineSyntaxText() ?? "{comparer}"}"
            : "";

        var parameterName = lambda.Symbol.Parameters[0].Name;
        var memberText = $"{parameterName} => {parameterName}.{string.Join(".", memberNames)}{(trailingNullForgiving ? "!" : "")}";

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, keySelectorText, comparerText, memberText));
    }
}
