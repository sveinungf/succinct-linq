using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class ConversionOperationExtensions
{
    extension(IConversionOperation conversion)
    {
        // Only conversions that are guaranteed to yield the exact same element.
        // Numeric, user-defined, and nullable conversions may change the element
        // (e.g. (double)(int)x), and explicit reference downcasts may throw
        // InvalidCastException (e.g. (string)x), so only implicit ones are kept.
        public bool PreservesElement
        {
            get
            {
                var data = conversion.Conversion;
                if (!data.Exists || data.IsUserDefined || data.IsNumeric || data.IsNullable)
                    return false;

                if (data.IsIdentity || (data.IsReference && data.IsImplicit))
                    return true;

                // Otherwise the conversion is element-preserving only if it
                // boxes or unboxes the value: the operand is a value type and
                // the target is object or an interface, or vice versa.
                if (conversion.Operand.Type is not { } source || conversion.Type is not { } target)
                    return false;

                if (IsBoxingTarget(source) && IsValueType(target))
                {
                    // An unboxing conversion preserves the element only when it
                    // reverses a boxing of the same value type, e.g. (T)(object)x.
                    // An unboxing without such an operand, such as (T)x in an
                    // IEnumerable<object>, can throw InvalidCastException.
                    return conversion.Operand is IConversionOperation
                        {
                            Operand.Type: { } boxed,
                            Type: { } boxedTarget
                        } &&
                        IsValueType(boxed) &&
                        IsBoxingTarget(boxedTarget) &&
                        SymbolEqualityComparer.IncludeNullability.Equals(boxed, target);
                }

                return IsValueType(source) && IsBoxingTarget(target);
            }
        }

        private static bool IsValueType(ITypeSymbol type) =>
            type.TypeKind is TypeKind.Struct or TypeKind.Enum ||
            type is ITypeParameterSymbol { HasValueTypeConstraint: true };

        private static bool IsBoxingTarget(ITypeSymbol type) =>
            type.TypeKind == TypeKind.Interface || type.SpecialType == SpecialType.System_Object;
    }
}
