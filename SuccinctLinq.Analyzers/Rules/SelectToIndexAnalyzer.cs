using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;
using System.Globalization;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SelectToIndexAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ202",
        title: "Select can be simplified",
        messageFormat: "Select can be simplified to Index({0})",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A Select that only returns the element and its index, such as (x, i) => (x, i) or (x, i) => new { x, i }, can be replaced with the more concise Index. Note that Index yields the index before the element.");

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
            !TryGetIndexOffset(select, out var startIndex))
        {
            return;
        }

        if (select.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var location = invocation.GetMethodCallLocation();
        var argument = startIndex == 0 ? "" : startIndex.ToString(CultureInfo.InvariantCulture);
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, argument));
    }

    private static bool TryGetIndexOffset(IInvocationOperation select, out int startIndex)
    {
        startIndex = 0;

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

        value = value.UnwrapConversions();

        IOperation? first = null;
        IOperation? second = null;

        if (value is ITupleOperation { Elements.Length: 2 } tuple)
        {
            first = tuple.Elements.ElementAtOrDefault(0);
            second = tuple.Elements.ElementAtOrDefault(1);
        }
        else if (value is IAnonymousObjectCreationOperation
        {
            Initializers: [IAssignmentOperation { } a, IAssignmentOperation { } b]
        })
        {
            first = a.Value;
            second = b.Value;
        }

        if (first is null || second is null)
            return false;

        if (second.ReferencesParameter(element) &&
            IsIndexWithOffset(first, index, out var offset))
        {
            startIndex = offset;
            return true;
        }

        return first.ReferencesParameter(element) && second.ReferencesParameter(index);
    }

    private static bool IsIndexWithOffset(IOperation operation, IParameterSymbol index, out int offset)
    {
        operation = operation.UnwrapConversions();
        offset = 0;

        if (operation.ReferencesParameter(index))
            return true;

        IOperation? constant = null;
        var isSubtraction = false;

        if (operation is IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } addition)
        {
            if (addition.LeftOperand.ReferencesParameter(index))
                constant = addition.RightOperand;
            else if (addition.RightOperand.ReferencesParameter(index))
                constant = addition.LeftOperand;
        }
        else if (operation is IBinaryOperation { OperatorKind: BinaryOperatorKind.Subtract } subtraction)
        {
            // Only a form such as (i - 1, x) can be expressed as Index(n); (1 - i, x) cannot.
            if (subtraction.LeftOperand.ReferencesParameter(index))
            {
                constant = subtraction.RightOperand;
                isSubtraction = true;
            }
        }

        if (constant is null)
            return false;

        constant = constant.UnwrapConversions();

        if (constant is not ILiteralOperation
            {
                Type.SpecialType: SpecialType.System_Int32,
                ConstantValue: { HasValue: true, Value: int value }
            })
        {
            return false;
        }

        offset = isSubtraction ? -value : value;
        return true;
    }
}
