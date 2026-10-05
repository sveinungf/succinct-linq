using Microsoft.CodeAnalysis.Operations;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class AnonymousFunctionOperationExtensions
{
    extension(IAnonymousFunctionOperation lambda)
    {
        public bool IsIdentityFunction()
        {
            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter, ..])
                return false;

            // A lambda that simply returns its first parameter compiles to a
            // block containing a single return operation. Extra parameters,
            // such as the index in Select((x, i) => x), may be ignored.
            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
                return false;

            // Only strip conversions that are guaranteed to yield the same
            // element; a cast such as (double)(int)x compiles to conversions
            // around a parameter reference but changes the value.
            return value.UnwrapPreservingConversions().ReferencesParameter(parameter);
        }

        // A lambda that returns the first element of its first parameter,
        // e.g. group => group.First(). Extra parameters, such as the index
        // in Select((group, i) => group.First()), may be ignored.
        public bool IsFirstFunction()
        {
            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter, ..])
                return false;

            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
                return false;

            return value.UnwrapPreservingConversions() is IInvocationOperation
            {
                TargetMethod.IsFirstMethod: true,
                Arguments: [var source]
            } && source.Value.UnwrapConversions().ReferencesParameter(parameter);
        }
    }
}
