using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class InvocationOperationExtensions
{
    extension(IInvocationOperation operation)
    {
        public IOperation? GetArgumentAtOrDefault(int index)
        {
            if (operation.TargetMethod.Parameters.Length <= index)
                return null;

            return operation.Arguments.ElementAtOrDefault(index)?.Value;
        }
    }
}
