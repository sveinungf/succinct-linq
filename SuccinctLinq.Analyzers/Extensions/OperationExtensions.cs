using Microsoft.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class OperationExtensions
{
    extension(IOperation operation)
    {
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
