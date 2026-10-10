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
        // The member names are returned in the order they are accessed, e.g. [Value] for group => group.First().Value.
        // Indexer access, such as group => group.First()[0], is not a member access.
        // Extra parameters, such as the index in Select((group, i) => group.First().Value), may be ignored.
        public bool TryGetMemberNamesOnFirstElement(
            [NotNullWhen(true)] out IReadOnlyList<string>? memberNames)
        {
            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter, ..])
            {
                memberNames = null;
                return false;
            }

            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
            {
                memberNames = null;
                return false;
            }

            // An indexer is a property reference, so it must be excluded.
            value = value.UnwrapPreservingConversions();
            if (value is not IMemberReferenceOperation { IsIndexerAccess: false } memberReference)
            {
                memberNames = null;
                return false;
            }

            // Walk down the member chain, collecting member names,
            // to the member whose instance is the First call.
            var names = new List<string>();
            var member = memberReference;

            while (true)
            {
                names.Add(member.Member.Name);

                if (member.Instance is IMemberReferenceOperation { IsIndexerAccess: false } inner)
                {
                    member = inner;
                    continue;
                }

                if (member.Instance is not { } firstCall || !firstCall.IsFirstMethodCallOnParameterType(parameter))
                {
                    memberNames = null;
                    return false;
                }

                names.Reverse();
                memberNames = names;
                return true;
            }
        }
    }
}
