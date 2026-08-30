using Microsoft.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class SymbolExtensions
{
    extension(ISymbol symbol)
    {
        public bool IsSystemLinqEnumerable => symbol is
        {
            Name: "Enumerable",
            ContainingNamespace:
            {
                Name: "Linq",
                ContainingNamespace:
                {
                    Name: "System",
                    ContainingNamespace.IsGlobalNamespace: true
                }
            }
        };

        public bool IsSystemCollectionsGenericNamespace => symbol is
        {
            Name: "Generic",
            ContainingNamespace:
            {
                Name: "Collections",
                ContainingNamespace:
                {
                    Name: "System",
                    ContainingNamespace.IsGlobalNamespace: true
                }
            }
        };

        public bool IsSystemCollectionsGenericIComparer => symbol is INamedTypeSymbol
        {
            Name: "IComparer",
            Arity: 1,
            ContainingNamespace.IsSystemCollectionsGenericNamespace: true
        };

        public bool IsSystemCollectionsGenericIEnumerable => symbol is INamedTypeSymbol
        {
            Name: "IEnumerable",
            Arity: 1,
            ContainingNamespace.IsSystemCollectionsGenericNamespace: true
        };

        public bool IsSystemCollectionsGenericIEqualityComparer => symbol is INamedTypeSymbol
        {
            Name: "IEqualityComparer",
            Arity: 1,
            ContainingNamespace.IsSystemCollectionsGenericNamespace: true
        };

        public bool IsSystemFuncWithArity2 => symbol is INamedTypeSymbol
        {
            Name: "Func",
            Arity: 2,
            ContainingNamespace:
            {
                Name: "System",
                ContainingNamespace.IsGlobalNamespace: true
            }
        };

        public bool IsSystemStringComparer => symbol is
        {
            Name: "StringComparer",
            ContainingNamespace:
            {
                Name: "System",
                ContainingNamespace.IsGlobalNamespace: true
            }
        };
    }
}
