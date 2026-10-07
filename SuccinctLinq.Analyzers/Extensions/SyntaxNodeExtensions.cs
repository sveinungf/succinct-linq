using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class SyntaxNodeExtensions
{
    extension(SyntaxNode node)
    {
        /// <summary>
        /// Collapses whitespace in the diagnostic message to a single space.
        /// Returns null if doing so would alter a string or character literal
        /// or a comment.
        /// </summary>
        public string? GetSingleLineText()
        {
            return node.ContainsLiteral() || node.ContainsComment()
                ? null
                : node.ToString().CollapseWhitespace();
        }

        private bool ContainsLiteral() =>
            node.DescendantTokens().Any(static token =>
                token.IsKind(SyntaxKind.StringLiteralToken) ||
                token.IsKind(SyntaxKind.CharacterLiteralToken) ||
                token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken) ||
                token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken) ||
                token.IsKind(SyntaxKind.InterpolatedStringTextToken));

        private bool ContainsComment() =>
            node.DescendantTokens().Any(static token =>
                token.LeadingTrivia.Any(static trivia => IsComment(trivia)) ||
                token.TrailingTrivia.Any(static trivia => IsComment(trivia)));
    }

    private static bool IsComment(SyntaxTrivia trivia) =>
        trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
        trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
}
