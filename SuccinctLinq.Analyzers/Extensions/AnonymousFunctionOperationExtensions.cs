using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class AnonymousFunctionOperationExtensions
{
    extension(IAnonymousFunctionOperation lambda)
    {
        /// <summary>
        /// A lambda that returns a expression that is likely safe to evaluate
        /// multiple times, e.g. x => x.Id.
        /// </summary>
        public bool IsPureExpression()
        {
            if (lambda.Symbol.IsAsync)
                return false;

            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
                return false;

            return value.IsPureExpression();
        }

        /// <summary>
        /// A lambda like "x => x.Count()".
        /// </summary>
        public bool IsCountMethod() =>
            lambda.IsSimpleSelector(static (value, parameter) => value.IsCountMethodOn(parameter));

        /// <summary>
        /// A lambda like "x => x.First()" or "x => x.FirstOrDefault()".
        /// </summary>
        public bool IsFirstMethod() =>
            lambda.IsSimpleSelector(static (value, parameter) => value.IsFirstMethodOn(parameter));

        /// <summary>
        /// A lambda like "x => x".
        /// </summary>
        public bool IsIdentityFunction() =>
            lambda.IsSimpleSelector(static (value, parameter) => value.UnwrapPreservingConversions().ReferencesParameter(parameter));

        /// <summary>
        /// A lambda like "x => x.Key".
        /// </summary>
        public bool IsKeyProperty() =>
            lambda.IsSimpleSelector(static (value, parameter) => value.IsKeyPropertyOn(parameter));

        /// <summary>
        /// A lambda like "x => x.Key" on a KeyValuePair.
        /// </summary>
        public bool IsKeyValuePairKeyProperty() =>
            lambda.IsSimpleSelector(static (value, parameter) => value.IsKeyValuePairPropertyOn(parameter, "Key"));

        /// <summary>
        /// A lambda like "x => x.Value" on a KeyValuePair.
        /// </summary>
        public bool IsKeyValuePairValueProperty() =>
            lambda.IsSimpleSelector(static (value, parameter) => value.IsKeyValuePairPropertyOn(parameter, "Value"));

        // A simple selector is a lambda that returns an expression that depends only on the parameter,
        // such as a method call or member access on the parameter (e.g. x => x.Count()).
        // Extra parameters, such as the index in Select((group, i) => group.Count()), is ignored.
        private bool IsSimpleSelector(Func<IOperation, IParameterSymbol, bool> predicate)
        {
            if (lambda.Symbol.IsAsync)
                return false;

            var parameters = lambda.Symbol.Parameters;
            if (parameters is not [var parameter, ..])
                return false;

            if (lambda.Body.Operations is not [IReturnOperation { ReturnedValue: { } value }])
                return false;

            return predicate(value, parameter);
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
                !firstCall.IsFirstMethodOn(parameter) ||
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

            // Concatenate token text without trivia so that multi-line chains
            // and comments yield a single-line member access.
            var builder = new StringBuilder();
            foreach (var token in syntax.DescendantTokens())
            {
                if (token.SpanStart < firstCallSpan.End)
                    continue;

                builder.Append(token.Text);
            }

            memberAccess = builder.ToString();
            return true;
        }

        // The source text of the lambda's first parameter, preserving any
        // escape character, e.g. "@class" for a parameter declared as "@class".
        public string? GetFirstParameterText()
        {
            return lambda.Syntax switch
            {
                SimpleLambdaExpressionSyntax { } simple => simple.Parameter.Identifier.Text,
                ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters: [var parameter, ..] } => parameter.Identifier.Text,
                AnonymousMethodExpressionSyntax { ParameterList.Parameters: [var parameter, ..] } => parameter.Identifier.Text,
                _ => null
            };
        }
    }
}
