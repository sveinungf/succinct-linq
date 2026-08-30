using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OrderByIdentityKeyAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ1101",
        title: "OrderBy(x => x) can be simplified",
        messageFormat: "OrderBy(x => x) can be simplified to Order()",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "An OrderBy() with the identity function (x => x) is equivalent to the more concise Order().");

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
        if (context.Operation is not IInvocationOperation orderBy ||
            !orderBy.TargetMethod.IsOrderByMethod ||
            !HasIdentityKeySelector(orderBy))
        {
            return;
        }

        if (orderBy.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var location = invocation.GetMethodCallLocation();
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
    }

    private static bool HasIdentityKeySelector(IInvocationOperation orderBy)
    {
        // The key type must equal the element type, otherwise the key
        // selector is not the identity function.
        var method = orderBy.TargetMethod;
        if (method.Arity < 2 ||
            !SymbolEqualityComparer.Default.Equals(method.TypeArguments[0], method.TypeArguments[1]))
        {
            return false;
        }

        var argument = orderBy.GetArgumentAtOrDefault(1);
        while (argument is IDelegateCreationOperation creation)
        {
            argument = creation.Target;
        }

        return argument is IAnonymousFunctionOperation lambda &&
            lambda.Symbol.Parameters.Length == 1 &&
            IsIdentityFunctionBody(lambda);
    }

    private static bool IsIdentityFunctionBody(IAnonymousFunctionOperation lambda)
    {
        // A lambda that simply returns its parameter compiles to a block
        // containing a single return operation.
        var operations = lambda.Body.Operations;
        if (operations is not [IReturnOperation { ReturnedValue: { } value }])
            return false;

        value = value.UnwrapConversions();

        return value is IParameterReferenceOperation { Parameter: { } reference }
            && SymbolEqualityComparer.Default.Equals(reference, lambda.Symbol.Parameters[0]);
    }
}
