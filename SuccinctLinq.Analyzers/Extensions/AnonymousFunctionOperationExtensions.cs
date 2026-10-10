using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics.CodeAnalysis;

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
        // e.g. group => group.First() or group => group.FirstOrDefault().
        // Extra parameters, such as the index in Select((group, i) => group.First()), may be ignored.
        public bool IsFirstMethod()
        {
            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter, ..])
                return false;

            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
                return false;

            return value.IsFirstMethodCallOnParameterType(parameter);
        }

        // A lambda that returns a member of the first element of its first parameter, e.g. group => group.First().Value.
        // Indexer access, such as group => group.First()[0], is not a member access.
        // Extra parameters, such as the index in Select((group, i) => group.First().Value), may be ignored.
        public bool IsFirstMethodFollowedByMemberAccess(
            [NotNullWhen(true)] out IMemberReferenceOperation? memberAccess)
        {
            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter, ..])
            {
                memberAccess = null;
                return false;
            }

            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
            {
                memberAccess = null;
                return false;
            }

            // An indexer is a property reference, so it must be excluded.
            value = value.UnwrapPreservingConversions();
            if (value is not IMemberReferenceOperation { IsIndexerAccess: false } memberReference)
            {
                memberAccess = null;
                return false;
            }

            // Walk down the member chain to the member whose instance is the First call.
            var instance = memberReference;
            while (instance.Instance is IMemberReferenceOperation { IsIndexerAccess: false } inner)
            {
                instance = inner;
            }

            if (instance.Instance is { } op && op.IsFirstMethodCallOnParameterType(parameter))
            {
                memberAccess = memberReference;
                return true;
            }

            memberAccess = null;
            return false;
        }
    }
}
