using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class InvocationOperationExtensions
{
    extension(IInvocationOperation operation)
    {
        public IOperation? GetArgumentAtOrDefault(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            var method = operation.TargetMethod;

            if (index >= method.Parameters.Length)
                return null;

            // The arguments are ordered as they appear in the source, so
            // out-of-order named arguments may not match the parameter
            // declaration order; resolve the argument by its parameter.
            var parameter = method.Parameters[index];

            foreach (var argument in operation.Arguments)
            {
                if (SymbolEqualityComparer.Default.Equals(argument.Parameter, parameter))
                    return argument.Value;
            }

            return null;
        }

        public bool HasIdentitySelector(int argumentIndex, SymbolEqualityComparer? comparer = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(argumentIndex, 0);

            var method = operation.TargetMethod;

            // The element type must equal the source type,
            // otherwise the selector is not the identity function.
            var actualComparer = comparer ?? SymbolEqualityComparer.IncludeNullability;

            if (method.TypeArguments.Length < argumentIndex + 1 ||
                !actualComparer.Equals(method.TypeArguments[0], method.TypeArguments[argumentIndex]))
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
