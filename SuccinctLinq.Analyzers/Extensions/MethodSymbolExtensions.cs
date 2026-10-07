using Microsoft.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class MethodSymbolExtensions
{
    extension(IMethodSymbol symbol)
    {
        public bool IsDistinctMethod => symbol is
        {
            Name: "Distinct",
            ContainingType.IsSystemLinqEnumerable: true
        };

        public bool IsOrderByMethod => symbol is
        {
            Name: "OrderBy",
            ContainingType.IsSystemLinqEnumerable: true,
            SelectorFirst: true,
            SelectorSecond: false
        };

        public bool IsOrderByDescendingMethod => symbol is
        {
            Name: "OrderByDescending",
            ContainingType.IsSystemLinqEnumerable: true,
            SelectorFirst: true,
            SelectorSecond: false
        };

        public bool IsFirstMethod => symbol is
        {
            Name: "First",
            ContainingType.IsSystemLinqEnumerable: true,
            Parameters.Length: 1
        };

        public bool IsFirstOrDefaultMethod => symbol is
        {
            Name: "FirstOrDefault",
            ContainingType.IsSystemLinqEnumerable: true,
            Parameters.Length: 1
        };

        public bool IsFirstOrFirstOrDefaultMethod => symbol.IsFirstMethod || symbol.IsFirstOrDefaultMethod;

        public bool IsToDictionaryWithElementSelectorMethod => symbol is
        {
            Name: "ToDictionary",
            ContainingType.IsSystemLinqEnumerable: true,
            SelectorFirst: true,
            SelectorSecond: true
        };

        public bool IsToHashSetMethod => symbol is
        {
            Name: "ToHashSet",
            ContainingType.IsSystemLinqEnumerable: true
        };

        public bool IsToLookupWithElementSelectorMethod => symbol is
        {
            Name: "ToLookup",
            ContainingType.IsSystemLinqEnumerable: true,
            SelectorFirst: true,
            SelectorSecond: true
        };

        public bool IsGroupByMethod => symbol is
        {
            Name: "GroupBy",
            ContainingType.IsSystemLinqEnumerable: true,
            SelectorFirst: true,
            SelectorSecond: false,
            HasResultSelector: false
        };

        public bool IsGroupByWithElementSelectorMethod => symbol is
        {
            Name: "GroupBy",
            ContainingType.IsSystemLinqEnumerable: true,
            SelectorFirst: true,
            SelectorSecond: true,
            HasResultSelector: false
        };

        private bool IsSelectMethod => symbol is
        {
            Name: "Select",
            ContainingType.IsSystemLinqEnumerable: true,
            SelectorFirst: true
        };

        public bool IsSelectWithTwoParametersMethod => symbol is
        {
            Name: "Select",
            ContainingType.IsSystemLinqEnumerable: true,
            SelectorFirstWithTwoParameters: true
        };

        public bool IsAnySelectMethod => symbol.IsSelectMethod || symbol.IsSelectWithTwoParametersMethod;

        private bool SelectorFirst => symbol.GetParameterAtOrDefault(1) is { Type.IsSystemFuncWithArity2: true };
        private bool SelectorFirstWithTwoParameters => symbol.GetParameterAtOrDefault(1) is { Type.IsSystemFuncWithArity3: true };
        private bool SelectorSecond => symbol.GetParameterAtOrDefault(2) is { Type.IsSystemFuncWithArity2: true };

        private bool HasResultSelector => symbol.Parameters.Any(p => p.Type.IsSystemFuncWithArity3);

        private IParameterSymbol? GetParameterAtOrDefault(int index) => symbol.Parameters.ElementAtOrDefault(index);
    }
}
