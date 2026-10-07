using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class SyntaxNodeExtensions
{
    extension(SyntaxNode node)
    {
        /// <summary>
        /// Collapses whitespace in the diagnostic message to a single space
        /// unless doing so would alter a string or character literal.
        /// </summary>
        public string GetSingleLineText()
        {
            var fullString = node.ToFullString();

            return node.ContainsLiteral()
                ? fullString
                : fullString.CollapseWhitespace();
        }

        private bool ContainsLiteral() =>
            node.DescendantTokens().Any(static token =>
                token.IsKind(SyntaxKind.StringLiteralToken) ||
                token.IsKind(SyntaxKind.CharacterLiteralToken) ||
                token.IsKind(SyntaxKind.InterpolatedStringExpression));
    }
}
