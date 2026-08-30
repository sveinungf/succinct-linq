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
            symbol.Parameters.Length == 1 ||
            symbol.Parameters.Length == 2 &&
            symbol.Parameters[1].Type.IsSystemCollectionsGenericIEqualityComparer;

        private bool HasKeySelectorParameters =>
            symbol.Parameters.Length is 2 or 3 &&
            symbol.Parameters[0].Type.IsSystemCollectionsGenericIEnumerable &&
            symbol.Parameters[1].Type.IsSystemFuncWithArity2;
    }
}
