using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantDistinctAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ1001",
        title: "Distinct() call is redundant",
        messageFormat: "The Distinct() call is redundant; the following ToHashSet() already removes duplicates",
        category: "Redundancy",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ToHashSet() already removes duplicate elements, so calling Distinct() immediately before it is redundant.");

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
            !toHashSet.TargetMethod.IsToHashSetMethod ||
            toHashSet.Arguments.Length == 0)
        {
            return;
        }

        var distinct = toHashSet.Arguments[0].Value switch
        {
            IInvocationOperation { TargetMethod.IsDistinctMethod: true } directInvocation => directInvocation,
            ILocalReferenceOperation localReference => GetDistinctInitializer(localReference),
            _ => null
        };

        if (distinct?.Syntax is not InvocationExpressionSyntax invocation)
            return;

        if (!UsesSameComparer(distinct, toHashSet))
            return;

        var location = invocation.GetMethodCallLocation();
        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
    }

    private static IInvocationOperation? GetDistinctInitializer(ILocalReferenceOperation toHashSetLocalReference)
    {
        // When the usage is inside a lambda or local function, abort further analysis.
        if (toHashSetLocalReference.IsInsideFunctionBoundary())
            return null;

        IOperation? lastWrite = null;
        var local = toHashSetLocalReference.Local;
        var usageStart = toHashSetLocalReference.Syntax.Span.Start;
        var root = toHashSetLocalReference.GetRootOperation();

        var stack = new Stack<(IOperation Operation, bool IsInsideFunctionBoundary)>();
        stack.Push((root, false));

        while (stack.Count > 0)
        {
            var (operation, isInsideFunctionBoundary) = stack.Pop();

            if (operation.ReadsLocalReference(toHashSetLocalReference))
            {
                // The local variable must not be read by anything else.
                return null;
            }

            // A write inside a lambda or local function body may not execute
            // before the usage, so it can't be the initializer.
            if (!isInsideFunctionBoundary &&
                TryGetWriteToLocal(operation, local, out var write) &&
                IsMostRecentWrite(write, usageStart, lastWrite))
            {
                lastWrite = write;
            }

            foreach (var child in operation.ChildOperations)
            {
                stack.Push((child, isInsideFunctionBoundary || operation.IsFunctionBoundary));
            }
        }

        if (lastWrite is not IInvocationOperation distinctInvocation ||
            !distinctInvocation.TargetMethod.IsDistinctMethod)
        {
            return null;
        }

        if (DistinctAndToHashSetInDifferentBranch(distinctInvocation, toHashSetLocalReference))
            return null;

        return distinctInvocation;
    }

    private static bool DistinctAndToHashSetInDifferentBranch(IOperation distinctInvocation, IOperation toHashSetLocalReference)
    {
        var operation = distinctInvocation.Parent;

        while (operation is not null)
        {
            if (operation.IsBranchBoundary)
            {
                // The write and the usage must be in the same branch of the
                // boundary; a usage in a sibling branch may execute without
                // the write having executed.
                var writeBranch = operation.ChildOperations
                    .FirstOrDefault(child => child.ContainsOperation(distinctInvocation));

                return writeBranch is not null && !writeBranch.ContainsOperation(toHashSetLocalReference);
            }

            operation = operation.Parent;
        }

        return false;
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

        distinctComparer = distinctComparer.UnwrapConversions();
        toHashSetComparer = toHashSetComparer.UnwrapConversions();

        // A null or default comparer argument falls back to the default
        // equality comparer, so both arguments use the same comparer.
        if (distinctComparer.IsNullOrDefault || toHashSetComparer.IsNullOrDefault)
            return distinctComparer.IsNullOrDefault && toHashSetComparer.IsNullOrDefault;

        // Only direct references to a built-in StringComparer member are
        // known to always denote the same comparer instance.
        return distinctComparer.TryGetStringComparerMember(out var distinctMember)
            && toHashSetComparer.TryGetStringComparerMember(out var toHashSetMember)
            && SymbolEqualityComparer.Default.Equals(distinctMember, toHashSetMember);
    }
}
