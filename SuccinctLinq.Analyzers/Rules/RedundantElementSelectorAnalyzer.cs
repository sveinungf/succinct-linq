using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantElementSelectorAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ102",
        title: "Redundant element selector",
        messageFormat: "Remove the redundant \"{0}\" selector",
        category: "Redundancy",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An element selector (x => x) that simply returns the source element is redundant; using the overload without an element selector is equivalent.");

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
        if (context.Operation is not IInvocationOperation call ||
            !call.TargetMethod.IsToDictionaryWithElementSelectorMethod &&
            !call.TargetMethod.IsToLookupWithElementSelectorMethod &&
            !call.TargetMethod.IsGroupByWithElementSelectorMethod ||
            !call.HasIdentitySelector(2))
        {
            return;
        }

        if (call.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var location = invocation.GetMethodCallLocation();
        var elementSelectorText = call.GetArgument(2)?.GetSingleLineSyntaxText() ?? "{elementSelector}";
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, elementSelectorText));
    }
}
