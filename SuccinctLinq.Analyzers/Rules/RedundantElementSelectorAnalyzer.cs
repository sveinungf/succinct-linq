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
        messageFormat: "The element selector (x => x) is redundant and can be removed",
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
            !call.TargetMethod.IsToDictionaryMethod &&
            !call.TargetMethod.IsToLookupMethod &&
            !call.TargetMethod.IsGroupByMethod ||
            !HasRedundantElementSelector(call))
        {
            return;
        }

        if (call.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var location = invocation.GetMethodCallLocation();
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
    }

    private static bool HasRedundantElementSelector(IInvocationOperation call)
    {
        var method = call.TargetMethod;

        // The element type must equal the source type (including nullability),
        // otherwise the element selector is not the identity function.
        if (method.Arity < 3 ||
            !SymbolEqualityComparer.IncludeNullability.Equals(method.TypeArguments[0], method.TypeArguments[2]))
        {
            return false;
        }

        var argument = call.GetArgumentAtOrDefault(2);
        while (argument is IDelegateCreationOperation creation)
        {
            argument = creation.Target;
        }

        return argument is IAnonymousFunctionOperation lambda && lambda.IsIdentityFunction();
    }
}
