using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class OperationExtensions
{
    extension(IOperation operation)
    {
        public bool IsBranchBoundary => operation is
            IConditionalOperation or ILoopOperation or ISwitchOperation or
            ISwitchExpressionOperation or ITranslatedQueryOperation;

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
    }
}
