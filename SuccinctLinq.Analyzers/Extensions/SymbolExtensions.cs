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

        public bool IsSystemCollectionsGenericIEqualityComparer => symbol is INamedTypeSymbol
        {
            Name: "IEqualityComparer",
            TypeParameters.Length: 1,
            ContainingNamespace:
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
            }
        };
    }
}
