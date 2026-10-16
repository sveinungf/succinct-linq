using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GroupByToDictionaryFirstMemberToDistinctByAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ211",
        title: "GroupBy followed by ToDictionary of Key and a member of First can be simplified",
        messageFormat: "Use DistinctBy({0}{1}).ToDictionary({0}{2}{3}) instead",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Grouping elements by a key and mapping each key to a member of the first element of its group keeps the first element of each distinct key and projects a member of it, so DistinctBy followed by ToDictionary expresses the same intent more concisely.");

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
        if (context.Operation is not IInvocationOperation { TargetMethod.IsToDictionaryWithElementSelectorMethod: true } toDictionary)
            return;

        var preceding = toDictionary.GetArgument(0)?.UnwrapPreservingConversions();
        if (preceding is not IInvocationOperation { TargetMethod.IsGroupByMethod: true } groupBy)
            return;

        if (!toDictionary.TryGetLambdaArgument(1, out var keySelectorLambda))
            return;

        if (!keySelectorLambda.IsKeyProperty())
            return;

        if (!toDictionary.TryGetLambdaArgument(2, out var elementSelectorLambda))
            return;

        if (!elementSelectorLambda.TryGetMemberNamesOnFirstElement(out var memberNames, out var trailingNullForgiving))
            return;

        if (groupBy.Syntax is not InvocationExpressionSyntax groupByInvocation ||
            toDictionary.Syntax is not InvocationExpressionSyntax toDictionaryInvocation)
        {
            return;
        }

        var location = groupByInvocation.GetMethodChainLocation(toDictionaryInvocation);

        var keySelectorText = groupBy.GetArgument(1)?.GetSingleLineSyntaxText() ?? "{keySelector}";

        // The comparer argument is optional.
        var groupComparerText = groupBy.GetArgument(2) is { } groupComparerArg
            ? $", {groupComparerArg.GetSingleLineSyntaxText() ?? "{comparer}"}"
            : "";

        var parameterName = elementSelectorLambda.Symbol.Parameters[0].Name;
        var elementSelectorText = $", {parameterName} => {parameterName}.{string.Join(".", memberNames)}{(trailingNullForgiving ? "!" : "")}";

        var dictionaryComparerText = toDictionary.GetArgument(3) is { } dictionaryComparerArg
            ? $", {dictionaryComparerArg.GetSingleLineSyntaxText() ?? "{comparer}"}"
            : "";

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, keySelectorText, groupComparerText, elementSelectorText, dictionaryComparerText));
    }
}
