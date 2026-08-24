using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantDistinctAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ1001",
        title: "Remove redundant Distinct() call",
        messageFormat: "Remove the redundant Distinct() call; ToHashSet() already removes duplicates",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Enumerable.ToHashSet() already removes duplicate elements, so calling Distinct() immediately before it is redundant.");

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
        if (context.Operation is not IInvocationOperation toHashSet ||
            !toHashSet.TargetMethod.IsToHashSetMethod)
        {
            return;
        }

        var receiver = toHashSet.Instance ?? toHashSet.Arguments.FirstOrDefault()?.Value;

        var distinct = receiver switch
        {
            IInvocationOperation { TargetMethod.IsDistinctMethod: true } directInvocation => directInvocation,
            ILocalReferenceOperation localReference => GetDistinctInitializer(localReference),
            _ => null
        };

        if (distinct?.Syntax is not InvocationExpressionSyntax invocation)
            return;

        if (!UsesSameComparer(distinct, toHashSet))
            return;

        var name = invocation.Expression;
        if (name is MemberAccessExpressionSyntax memberAccess)
            name = memberAccess.Name;

        var location = Location.Create(
            invocation.SyntaxTree,
            new TextSpan(name.Span.Start, invocation.Span.End - name.Span.Start));

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
    }

    private static IInvocationOperation? GetDistinctInitializer(ILocalReferenceOperation localReference)
    {
        var local = localReference.Local;
        var usageStart = localReference.Syntax.Span.Start;

        IOperation? lastWrite = null;
        var hasExtraRead = false;

        var stack = new Stack<IOperation>();
        stack.Push(localReference.GetRootOperation());
        while (stack.Count > 0 && !hasExtraRead)
        {
            var operation = stack.Pop();

            if (IsOtherReadOfLocal(operation, local, localReference))
            {
                // The local variable must not be read by anything else.
                hasExtraRead = true;
            }
            else if (TryGetWriteToLocal(operation, local, out var write) &&
                     IsMostRecentWrite(write, usageStart, lastWrite))
            {
                lastWrite = write;
            }

            foreach (var child in operation.ChildOperations)
            {
                stack.Push(child);
            }
        }

        if (hasExtraRead ||
            lastWrite is not IInvocationOperation distinctInvocation ||
            !distinctInvocation.TargetMethod.IsDistinctMethod)
        {
            return null;
        }

        return distinctInvocation;
    }

    private static bool IsOtherReadOfLocal(
        IOperation operation,
        ILocalSymbol local,
        ILocalReferenceOperation usage)
    {
        if (operation is not ILocalReferenceOperation reference ||
            !SymbolEqualityComparer.Default.Equals(reference.Local, local) ||
            ReferenceEquals(reference, usage))
        {
            return false;
        }

        if (operation.Parent is IAssignmentOperation assignment &&
            ReferenceEquals(assignment.Target, reference))
        {
            return false;
        }

        return true;
    }

    private static bool TryGetWriteToLocal(
        IOperation operation,
        ILocalSymbol local,
        [NotNullWhen(true)] out IOperation? write)
    {
        if (operation is IAssignmentOperation assignment &&
            assignment.Target is ILocalReferenceOperation target &&
            SymbolEqualityComparer.Default.Equals(target.Local, local))
        {
            write = assignment.Value;
            return true;
        }

        if (operation is IVariableDeclarationOperation declaration)
        {
            var declarator = declaration.Declarators.FirstOrDefault(d =>
                SymbolEqualityComparer.Default.Equals(d.Symbol, local));
            if (declarator?.Initializer is { } initializer)
            {
                write = initializer.Value;
                return true;
            }
        }

        write = null;
        return false;
    }

    private static bool IsMostRecentWrite(IOperation write, int usageStart, IOperation? current)
    {
        var start = write.Syntax.Span.Start;
        return start < usageStart && (current is null || start > current.Syntax.Span.Start);
    }

    private static bool UsesSameComparer(IInvocationOperation distinct, IInvocationOperation toHashSet)
    {
        var distinctComparer = distinct.GetArgumentAtOrDefault(1);
        var toHashSetComparer = toHashSet.GetArgumentAtOrDefault(1);

        if (distinctComparer is null || toHashSetComparer is null)
            return distinctComparer is null && toHashSetComparer is null;

        return IsSameOperation(distinctComparer, toHashSetComparer);
    }

    private static bool IsSameOperation(IOperation left, IOperation right)
    {
        while (left is IConversionOperation leftConversion)
            left = leftConversion.Operand;
        while (right is IConversionOperation rightConversion)
            right = rightConversion.Operand;

        // Each invocation or object creation is re-evaluated and could
        // produce a different comparer instance.
        if (left is IInvocationOperation or IObjectCreationOperation ||
            right is IInvocationOperation or IObjectCreationOperation)
        {
            return false;
        }

        var leftSymbol = GetSymbol(left);
        var rightSymbol = GetSymbol(right);
        if (leftSymbol is not null || rightSymbol is not null)
            return SymbolEqualityComparer.Default.Equals(leftSymbol, rightSymbol);

        return left.Syntax.SyntaxTree == right.Syntax.SyntaxTree &&
            string.Equals(
                left.Syntax.NormalizeWhitespace().ToFullString(),
                right.Syntax.NormalizeWhitespace().ToFullString(),
                StringComparison.Ordinal);
    }

    private static ISymbol? GetSymbol(IOperation operation) => operation switch
    {
        IParameterReferenceOperation parameterReference => parameterReference.Parameter,
        ILocalReferenceOperation localReference => localReference.Local,
        IMemberReferenceOperation memberReference => memberReference.Member,
        _ => null
    };
}
