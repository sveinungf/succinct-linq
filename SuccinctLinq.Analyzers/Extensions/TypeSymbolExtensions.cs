using Microsoft.CodeAnalysis;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class TypeSymbolExtensions
{
    extension(ITypeSymbol type)
    {
        public bool PreservesNullability(ITypeSymbol other)
        {
            if (!SymbolEqualityComparer.Default.Equals(type, other))
                return false;

            return !type.IsReferenceType
                || other.NullableAnnotation is not NullableAnnotation.Annotated
                || type.NullableAnnotation is NullableAnnotation.Annotated;
        }
    }
}
