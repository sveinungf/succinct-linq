using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            if (lambda.Symbol.IsAsync)
                return false;

            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter, ..])
                return false;

            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
                return false;

            return value.IsFirstMethodCallOnParameterType(parameter);
        }

        // A lambda that returns the Key property of its first parameter,
        // e.g. group => group.Key.
        // Extra parameters, such as the index in Select((group, i) => group.Key), may be ignored.
        public bool IsKeyProperty()
        {
            if (lambda.Symbol.IsAsync)
                return false;

            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter, ..])
                return false;

            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
                return false;

            return value.UnwrapIdentityConversions() is IMemberReferenceOperation
            {
                IsIndexerAccess: false,
                Member: IPropertySymbol { Name: "Key", ContainingType.IsSystemLinqIGrouping: true },
                Instance: { } instance
            } && instance.UnwrapPreservingConversions().ReferencesParameter(parameter);
        }

        // A lambda that returns a member of the first element of its first parameter, e.g. group => group.First().Value.
        // The member access is returned relative to the first element, including any null-forgiving operators,
        // e.g. ".Value" for group => group.First().Value, ".Value!" for group => group.First().Value!,
        // and ".Address!.City" for group => group.First().Address!.City.
        // Indexer access, such as group => group.First()[0], is not a member access.
        // Extra parameters, such as the index in Select((group, i) => group.First().Value), may be ignored.
        public bool TryGetMemberAccessOnFirstElement([NotNullWhen(true)] out string? memberAccess)
        {
            memberAccess = null;
            if (lambda.Symbol.IsAsync)
                return false;

            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter, ..])
                return false;

            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
                return false;

            // An indexer is a property reference, so it must be excluded.
            value = value.UnwrapIdentityConversions();
            if (value is not IMemberReferenceOperation { IsIndexerAccess: false } memberReference)
                return false;

            // Walk down the member chain to the member whose instance is the First call.
            var member = memberReference;
            while (member.Instance is IMemberReferenceOperation { IsIndexerAccess: false } inner)
            {
                member = inner;
            }

            if (member.Instance is not { } firstCall ||
                !firstCall.IsFirstMethodCallOnParameterType(parameter) ||
                memberReference.Syntax is not { } syntax ||
                firstCall.Syntax is not { } firstCallSyntax)
            {
                return false;
            }

            var syntaxSpan = syntax.Span;
            var firstCallSpan = firstCallSyntax.Span;
            if (syntaxSpan.Start != firstCallSpan.Start || firstCallSpan.End > syntaxSpan.End)
                return false;

            // A trailing null-forgiving operator, such as the "!" in
            // group.First().Value!, is a postfix unary parent of the member
            // access, so extend the syntax through it.
            while (syntax.Parent is PostfixUnaryExpressionSyntax unary &&
                unary.OperatorToken.IsKind(SyntaxKind.ExclamationToken))
            {
                syntax = unary;
            }

            // The instance of a member access is a prefix of the member access,
            // so the member access relative to the first element is the outer
            // text with the First call removed, e.g. ".Address!.City" for
            // group.First().Address!.City.
            var syntaxText = syntax.ToString();
            var start = firstCallSpan.End - syntaxSpan.Start;
            var span = syntaxText.AsSpan(start).TrimStart();

            // A null-forgiving operator on the First call itself, such as the
            // "!" in group.First()!.Value, is not part of the member chain.
            if (span.StartsWith('!'))
                span = span[1..];
            memberAccess = span.ToString();
            return true;
        }
    }
}
