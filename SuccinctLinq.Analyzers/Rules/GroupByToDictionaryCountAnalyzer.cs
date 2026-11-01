using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GroupByToDictionaryCountAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ213",
        title: "GroupBy followed by ToDictionary of Key and Count can be simplified",
        messageFormat: "Use CountBy({0}{1}).ToDictionary({2}) instead",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Grouping elements by a key and counting elements in each group is exactly what CountBy produces. CountBy followed by ToDictionary expresses the same intent more concisely.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            // The CountBy() method is only available in .NET 9 and later.
            if (startContext.Compilation.IsTargetFrameworkAtLeast(9))
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

        if (!toDictionary.TryGetLambdaArgument(1, out var keySelectorLambda) || !keySelectorLambda.IsKeyProperty())
            return;

        if (!toDictionary.TryGetLambdaArgument(2, out var elementSelectorLambda) || !elementSelectorLambda.IsCountMethod())
            return;

        if (groupBy.Syntax is not InvocationExpressionSyntax groupByInvocation ||
            toDictionary.Syntax is not InvocationExpressionSyntax toDictionaryInvocation)
        {
            return;
        }

        var location = groupByInvocation.GetMethodChainLocation(toDictionaryInvocation);

        var keySelectorText = groupBy.GetArgument(1)?.GetSingleLineSyntaxText() ?? "{keySelector}";

        // The comparer arguments are optional.
        var groupComparerText = groupBy.GetComparerArgumentSyntaxText(2, prefix: ", ");
        var dictionaryComparerText = toDictionary.GetComparerArgumentSyntaxText(3, prefix: "");

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, keySelectorText, groupComparerText, dictionaryComparerText));
    }
}
