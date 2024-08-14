using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyModel;

namespace RobotAppLibrary.StrategyDynamicCompiler;

public static class StrategyDynamiqCompiler
{
    public static byte[] TryCompileSourceCode(string sourceCode)
    {
        sourceCode = EnsureUsingDirective(sourceCode);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        
        var dependencyContext = DependencyContext.Default;
        var runtimeAssemblies = dependencyContext.RuntimeLibraries
            .SelectMany(library => library.GetDefaultAssemblyNames(dependencyContext))
            .Select(Assembly.Load)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();
        
        var systemRuntimeLocation = Assembly.Load(new AssemblyName("System")).Location;
        runtimeAssemblies.Add(MetadataReference.CreateFromFile(systemRuntimeLocation));

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release)
            .WithOverflowChecks(false);

        var compilation = CSharpCompilation.Create("StrategyDynamicAssembly")
            .AddReferences(runtimeAssemblies)
            .AddSyntaxTrees(syntaxTree)
            .WithOptions(compilationOptions);

        using var ms = new MemoryStream();
        var compileResult = compilation.Emit(ms);

        if (compileResult.Success) return ms.ToArray();
        IEnumerable<Diagnostic> compileErrors = compileResult.Diagnostics;
        throw new CompilationException("La compilation a échoué.",
            compileErrors.Where(error => error.Severity == DiagnosticSeverity.Error));
    }

    public static string? GetFirstClassName(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = syntaxTree.GetRoot() as CompilationUnitSyntax;

        var firstClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (firstClass == null) return null;

        var namespaceDeclaration = firstClass.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceDeclaration != null
            ? $"{namespaceDeclaration.Name}.{firstClass.Identifier.ValueText}"
            : firstClass.Identifier.ValueText;
    }
    
    private static string EnsureUsingDirective(string sourceCode, string namespaceName = "System")
    {
        if (!sourceCode.StartsWith($"using {namespaceName};"))
        {
            sourceCode = $"using {namespaceName};\n{sourceCode}";
        }
        return sourceCode;
    }
}