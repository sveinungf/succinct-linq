using Microsoft.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class MethodSymbolExtensions
{
    extension(IMethodSymbol symbol)
    {
        public bool IsDistinctMethod => symbol is
        {
            Name: "Distinct",
            ContainingType.IsSystemLinqEnumerable: true,
            HasExpectedParameters: true
        };

        public bool IsToHashSetMethod => symbol is
        {
            Name: "ToHashSet",
            ContainingType.IsSystemLinqEnumerable: true,
            HasExpectedParameters: true
        };

        private bool HasExpectedParameters =>
            symbol.Parameters.Length == 1 ||
            symbol.Parameters.Length == 2 &&
            symbol.Parameters[1].Type.IsSystemCollectionsGenericIEqualityComparer;
    }
}
