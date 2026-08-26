using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class OperationExtensions
{
    extension(IOperation operation)
    {
        public bool IsBranchBoundary => operation is
            IConditionalOperation or ILoopOperation or ISwitchOperation or
            ISwitchExpressionOperation or ITranslatedQueryOperation or ITryOperation or
            ICoalesceOperation or IConditionalAccessOperation or
            IBinaryOperation { OperatorKind: BinaryOperatorKind.ConditionalAnd or BinaryOperatorKind.ConditionalOr };

        public bool IsFunctionBoundary => operation is
            IAnonymousFunctionOperation or ILocalFunctionOperation;

        public bool IsNullOrDefault => operation switch
        {
            IDefaultValueOperation => true,
            ILiteralOperation literal => literal.ConstantValue.HasValue && literal.ConstantValue.Value is null,
            _ => false
        };

        public bool ContainsOperation(IOperation other)
        {
            for (var node = other.Parent; node is not null; node = node.Parent)
            {
                if (ReferenceEquals(node, operation))
                    return true;
            }

            return false;
        }

        public IOperation GetRootOperation()
        {
            while (operation.Parent is not null)
            {
                operation = operation.Parent;
            }

            return operation;
        }

        public bool IsInsideFunctionBoundary()
        {
            var node = operation.Parent;

            while (node is not null)
            {
                if (node.IsFunctionBoundary)
                    return true;

                node = node.Parent;
            }

            return false;
        }

        public bool ReadsLocalReference(ILocalReferenceOperation localReference)
        {
            if (operation is ILocalReferenceOperation reference &&
                SymbolEqualityComparer.Default.Equals(reference.Local, localReference.Local) &&
                !ReferenceEquals(reference, localReference))
            {
                return operation.Parent is not IAssignmentOperation assignment ||
                    !ReferenceEquals(assignment.Target, reference);
            }

            return false;
        }

        public bool TryGetStringComparerMember([NotNullWhen(true)] out ISymbol? member)
        {
            if (operation is IMemberReferenceOperation { Instance: null } memberReference &&
                memberReference.Member is IPropertySymbol or IFieldSymbol &&
                memberReference.Member.ContainingType.IsSystemStringComparer)
            {
                member = memberReference.Member;
                return true;
            }

            member = null;
            return false;
        }
    }
}
