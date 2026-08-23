using Microsoft.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class MethodSymbolExtensions
{
    extension(IMethodSymbol symbol)
    {
        public bool IsDistinctMethod => symbol is
        {
            Name: "Distinct",
            Parameters.Length: 1,
            ContainingType.IsSystemLinqEnumerable: true
        };

        public bool IsToHashSetMethod => symbol is
        {
            Name: "ToHashSet",
            Parameters.Length: 1,
            ContainingType.IsSystemLinqEnumerable: true
        };
    }
}
