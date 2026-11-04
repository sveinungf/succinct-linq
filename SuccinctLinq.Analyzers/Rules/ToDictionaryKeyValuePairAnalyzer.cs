using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ToDictionaryKeyValuePairAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ104",
        title: "Redundant Key and Value selectors in ToDictionary",
        messageFormat: "Use ToDictionary({0}) instead",
        category: "Redundancy",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calling ToDictionary(x => x.Key, x => x.Value) on a sequence of KeyValuePair is redundant; the parameterless ToDictionary() is equivalent.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(startContext =>
        {
            // The parameterless ToDictionary() overload is only available in .NET 8 and later.
            if (startContext.Compilation.IsTargetFrameworkAtLeast(8))
                startContext.RegisterOperationAction(Analyze, OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation { TargetMethod.IsToDictionaryWithElementSelectorMethod: true } toDictionary)
            return;

        if (toDictionary.TargetMethod.TypeArguments[0] is not INamedTypeSymbol { IsSystemCollectionsGenericKeyValuePair: true } keyValuePair)
            return;

        if (!toDictionary.TryGetLambdaArgument(1, out var keySelector) ||
            !keySelector.IsKeyValuePairKeyProperty())
        {
            return;
        }

        if (!toDictionary.TryGetLambdaArgument(2, out var elementSelector) ||
            !elementSelector.IsKeyValuePairValueProperty())
        {
            return;
        }

        // The parameterless overload infers the key and value types from the
        // source, so the selectors must not change the nullability of the
        // resulting dictionary, e.g. x => x.Value! on a nullable value, and
        // explicit type arguments must not change the value type.
        if (!keySelector.Symbol.ReturnType.PreservesNullability(keyValuePair.TypeArguments[0]) ||
            !elementSelector.Symbol.ReturnType.PreservesNullability(keyValuePair.TypeArguments[1]) ||
            !elementSelector.Symbol.ReturnType.PreservesNullability(toDictionary.TargetMethod.TypeArguments[2]))
        {
            return;
        }

        if (toDictionary.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var location = invocation.GetMethodCallLocation();

        // The comparer argument is optional.
        var comparerText = toDictionary.GetComparerArgumentSyntaxText(3, prefix: null);

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, comparerText));
    }
}
