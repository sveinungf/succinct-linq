using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class InvocationOperationExtensions
{
    extension(IInvocationOperation operation)
    {
        public IOperation? GetArgumentAtOrDefault(int index)
        {
            return operation.TargetMethod.Parameters.Length > index
                ? operation.Arguments.ElementAtOrDefault(index)
                : null;
        }
    }
}
