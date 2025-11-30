using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Rnwood.Dataverse.Data.PowerShell.Analyzers
{
    /// <summary>
    /// Analyzer that detects direct usage of IOrganizationService methods that should use
    /// the throttling-protected QueryHelpers wrapper methods instead.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseThrottlingProtectedMethodsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DVPS001";
        private const string Category = "Reliability";

        private static readonly LocalizableString Title = 
            "Use throttling-protected method";
        
        private static readonly LocalizableString MessageFormat = 
            "Direct call to '{0}' should use the throttling-protected 'QueryHelpers.{1}' method instead to handle service protection limits";
        
        private static readonly LocalizableString Description = 
            "Direct IOrganizationService method calls can fail with service protection throttling errors. Use the QueryHelpers wrapper methods that handle automatic retry.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        // Methods to detect and their suggested replacements
        private static readonly ImmutableDictionary<string, string> MethodMappings = new Dictionary<string, string>
        {
            { "Execute", "ExecuteWithThrottlingRetry" },
            { "Create", "CreateWithThrottlingRetry" },
            { "Update", "UpdateWithThrottlingRetry" },
            { "Delete", "DeleteWithThrottlingRetry" },
            { "Retrieve", "RetrieveWithThrottlingRetry" },
            { "RetrieveMultiple", "RetrieveMultipleWithThrottlingRetry" },
            { "Associate", "AssociateWithThrottlingRetry" },
            { "Disassociate", "DisassociateWithThrottlingRetry" }
        }.ToImmutableDictionary();

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Get the method being called
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            var methodName = memberAccess.Name.Identifier.ValueText;

            // Check if this is one of the methods we care about
            if (!MethodMappings.TryGetValue(methodName, out var suggestedMethod))
                return;

            // Get the symbol for the method being called
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return;

            // Check if the containing type is IOrganizationService or implements it
            var containingType = methodSymbol.ContainingType;
            if (containingType == null)
                return;

            // Check if the type is IOrganizationService or a class that implements it
            // We need to handle both interface calls and concrete class calls (like ServiceClient)
            if (!IsOrganizationServiceType(containingType))
                return;

            // Exclude calls already within QueryHelpers class (they're the implementation)
            var containingClass = GetContainingClass(invocation, context.SemanticModel);
            if (containingClass != null)
            {
                var classSymbol = context.SemanticModel.GetDeclaredSymbol(containingClass) as INamedTypeSymbol;
                if (classSymbol != null)
                {
                    // Exclude classes that implement IOrganizationService (they ARE service implementations)
                    if (ImplementsOrganizationService(classSymbol))
                        return;
                    
                    // Exclude QueryHelpers class
                    if (classSymbol.Name == "QueryHelpers")
                        return;
                }
            }

            // Also exclude calls within the *WithThrottlingRetry methods themselves
            var containingMethod = GetContainingMethod(invocation);
            if (containingMethod != null)
            {
                var currentMethodName = containingMethod.Identifier.ValueText;
                if (currentMethodName.EndsWith("WithThrottlingRetry"))
                    return;
                
                // Exclude methods that handle throttling in their catch blocks (batch processors)
                if (currentMethodName == "ExecuteBatch" || 
                    currentMethodName == "ExecuteWithRetry" ||
                    currentMethodName == "ProcessWorkerQueue")
                    return;
            }

            // Report diagnostic
            var diagnostic = Diagnostic.Create(
                Rule,
                memberAccess.Name.GetLocation(),
                methodName,
                suggestedMethod);

            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsOrganizationServiceType(INamedTypeSymbol type)
        {
            // Check the type name first
            if (type.Name == "IOrganizationService")
                return true;

            // Check if it's ServiceClient or any class that implements IOrganizationService
            if (type.Name == "ServiceClient" || type.Name == "OrganizationServiceProxy")
                return true;

            // Check if it implements IOrganizationService
            foreach (var iface in type.AllInterfaces)
            {
                if (iface.Name == "IOrganizationService")
                    return true;
            }

            return false;
        }

        private static bool ImplementsOrganizationService(INamedTypeSymbol type)
        {
            // Check if the class itself is IOrganizationService implementation
            foreach (var iface in type.AllInterfaces)
            {
                if (iface.Name == "IOrganizationService")
                    return true;
            }
            return false;
        }

        private static ClassDeclarationSyntax GetContainingClass(SyntaxNode node, SemanticModel semanticModel)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is ClassDeclarationSyntax classDecl)
                    return classDecl;
                current = current.Parent;
            }
            return null;
        }

        private static MethodDeclarationSyntax GetContainingMethod(SyntaxNode node)
        {
            var current = node.Parent;
            while (current != null)
            {
                if (current is MethodDeclarationSyntax methodDecl)
                    return methodDecl;
                current = current.Parent;
            }
            return null;
        }
    }
}
