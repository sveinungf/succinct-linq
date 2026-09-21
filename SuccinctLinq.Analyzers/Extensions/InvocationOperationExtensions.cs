using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class InvocationOperationExtensions
{
    extension(IInvocationOperation operation)
    {
        public IOperation? GetArgumentAtOrDefault(int index) => operation.Arguments.ElementAtOrDefault(index)?.Value;

        public bool HasIdentitySelector(int argumentIndex)
        {
            var method = operation.TargetMethod;

            // The element type must equal the source type (including nullability),
            // otherwise the selector is not the identity function.
            if (method.TypeArguments.Length < argumentIndex + 1 ||
                !SymbolEqualityComparer.IncludeNullability.Equals(method.TypeArguments[0], method.TypeArguments[argumentIndex]))
            {
                return false;
            }

            var argument = operation.GetArgumentAtOrDefault(argumentIndex);
            while (argument is IDelegateCreationOperation creation)
            {
                argument = creation.Target;
            }

            return argument is IAnonymousFunctionOperation lambda && lambda.IsIdentityFunction();
        }
    }
}
