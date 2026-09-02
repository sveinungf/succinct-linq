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
            HasOptionalComparerParameter: true
        };

        public bool IsToHashSetMethod => symbol is
        {
            Name: "ToHashSet",
            ContainingType.IsSystemLinqEnumerable: true,
            HasOptionalComparerParameter: true
        };

        public bool IsOrderByMethod => symbol is
        {
            Name: "OrderBy",
            ContainingType.IsSystemLinqEnumerable: true,
            HasKeySelectorParameters: true
        };

        private bool HasOptionalComparerParameter =>
            symbol.Parameters.Length is 1 or 2
            && symbol.GetParameterAtOrDefault(1) is null or { Type.IsSystemCollectionsGenericIEqualityComparer: true };

        private bool HasKeySelectorParameters =>
            symbol.GetParameterAtOrDefault(2) is null or { Type.IsSystemCollectionsGenericIComparer: true }
            && symbol.Parameters is { Length: 2 or 3 } and
            [
                { Type.IsSystemCollectionsGenericIEnumerable: true },
                { Type.IsSystemFuncWithArity2: true },
                ..
            ];

        private IParameterSymbol? GetParameterAtOrDefault(int index) => symbol.Parameters.ElementAtOrDefault(index);
    }
}
