using Microsoft.CodeAnalysis.Operations;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class AnonymousFunctionOperationExtensions
{
    extension(IAnonymousFunctionOperation lambda)
    {
        public bool IsIdentityFunction()
        {
            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter])
                return false;

            // A lambda that simply returns its parameter compiles to a block
            // containing a single return operation.
            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
                return false;

            return value.UnwrapConversions().ReferencesParameter(parameter);
        }
    }
}
