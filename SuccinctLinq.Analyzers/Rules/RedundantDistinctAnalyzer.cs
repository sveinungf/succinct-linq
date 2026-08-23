using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using SuccinctLinq.Analyzers.Extensions;
using System.Collections.Immutable;

namespace SuccinctLinq.Analyzers.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantDistinctAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Descriptor = new(
        id: "SLQ1001",
        title: "Remove redundant Distinct() call",
        messageFormat: "Remove the redundant Distinct() call; ToHashSet() already removes duplicates",
        category: "Simplification",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Enumerable.ToHashSet() already removes duplicate elements, so calling Distinct() immediately before it is redundant.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(Analyze, OperationKind.Invocation);
    }

    private static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation toHashSet ||
            !toHashSet.TargetMethod.IsToHashSetMethod)
        {
            return;
        }

        var receiver = toHashSet.Instance ?? toHashSet.Arguments.FirstOrDefault()?.Value;
        if (receiver is not IInvocationOperation distinct ||
            !distinct.TargetMethod.IsDistinctMethod)
        {
            return;
        }

        if (distinct.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var name = invocation.Expression;
        if (name is MemberAccessExpressionSyntax memberAccess)
            name = memberAccess.Name;
        var location = Location.Create(
            invocation.SyntaxTree,
            new TextSpan(name.Span.Start, invocation.Span.End - name.Span.Start));

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
    }
}
