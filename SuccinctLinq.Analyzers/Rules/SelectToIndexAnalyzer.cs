using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SelectToIndexAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ1102",
        title: "Select can be simplified",
        messageFormat: "Select can be simplified to Index()",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A Select with a selector that returns the element and its index as a tuple, such as (x, i) => (x, i), can be replaced with the more concise Index(); note that Index() yields the index before the element.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            // The Index() method is only available in .NET 9 and later.
            if (startContext.Compilation.IsTargetFrameworkAtLeast(9))
                startContext.RegisterOperationAction(Analyze, OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation select ||
            !select.TargetMethod.IsSelectMethod ||
            !IsIdentityTupleSelector(select))
        {
            return;
        }

        if (select.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var location = invocation.GetMethodCallLocation();
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
    }

    private static bool IsIdentityTupleSelector(IInvocationOperation select)
    {
        var argument = select.GetArgumentAtOrDefault(1);
        while (argument is IDelegateCreationOperation creation)
        {
            argument = creation.Target;
        }

        if (argument is not IAnonymousFunctionOperation lambda ||
            lambda.Symbol.Parameters is not [var element, var index])
        {
            return false;
        }

        // A lambda that simply returns its parameters compiles to a block
        // containing a single return operation.
        if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
        {
            return false;
        }

        if (value.UnwrapConversions() is not ITupleOperation { Elements: [var first, var second] })
        {
            return false;
        }

        // Index() yields the index before the element. Tuple element names
        // are irrelevant because the caller can rename them at the use site.
        return (IsParameterReference(first, index) && IsParameterReference(second, element)) ||
            (IsParameterReference(first, element) && IsParameterReference(second, index));
    }

    private static bool IsParameterReference(IOperation operation, IParameterSymbol parameter)
    {
        operation = operation.UnwrapConversions();
        return operation is IParameterReferenceOperation { Parameter: { } reference } &&
            SymbolEqualityComparer.Default.Equals(reference, parameter);
    }
}
