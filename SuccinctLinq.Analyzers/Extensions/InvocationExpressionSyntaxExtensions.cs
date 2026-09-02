using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class InvocationExpressionSyntaxExtensions
{
    extension(InvocationExpressionSyntax invocation)
    {
        public Location GetMethodCallLocation()
        {
            var name = invocation.Expression;
            if (name is MemberAccessExpressionSyntax memberAccess)
                name = memberAccess.Name;

            var span = new TextSpan(name.Span.Start, invocation.Span.End - name.Span.Start);
            return Location.Create(invocation.SyntaxTree, span);
        }
    }
}
