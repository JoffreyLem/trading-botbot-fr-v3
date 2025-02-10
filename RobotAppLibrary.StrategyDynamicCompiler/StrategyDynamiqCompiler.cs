using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyModel;
using RobotAppLibrary.Chart;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles.Attribute;
using RobotAppLibrary.Strategy;

namespace RobotAppLibrary.StrategyDynamicCompiler;

public static class StrategyDynamicCompiler
{
    public static (byte[] compiledAssembly, string? name, string? version) TryCompileSourceCode(
        string sourceCode, bool verifyAssembly = true)
    {
        var compileErrors = new List<Diagnostic>();

        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            PerformSecurityChecks(syntaxTree, compileErrors);

            var compilation = CreateCompilation(syntaxTree);
            using var ms = new MemoryStream();
            var compileResult = compilation.Emit(ms);

            if (!compileResult.Success)
            {
                compileErrors.AddRange(compileResult.Diagnostics);
                throw new CompilationException("La compilation a échoué.", 
                    compileErrors.Where(e => e.Severity == DiagnosticSeverity.Error));
            }

            var compiledData = ms.ToArray();
            return verifyAssembly ? VerifyCompiledAssembly(compiledData, compileErrors) : (compiledData, null, null);
        }
        finally
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private static CSharpCompilation CreateCompilation(SyntaxTree syntaxTree)
    {
        var references = GetAssemblyReferences();
        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release)
            .WithOverflowChecks(false)
            .WithAllowUnsafe(false);

        return CSharpCompilation.Create("StrategyDynamicAssembly")
            .AddReferences(references)
            .AddSyntaxTrees(syntaxTree)
            .WithOptions(compilationOptions);
    }

    private static List<MetadataReference> GetAssemblyReferences()
    {
        var dependencyContext = DependencyContext.Default;
        var references = dependencyContext.RuntimeLibraries
            .SelectMany(library => library.GetDefaultAssemblyNames(dependencyContext))
            .Select(Assembly.Load)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToList();

        references.Add(MetadataReference.CreateFromFile(Assembly.Load("System").Location));
        return [..references];
    }

    public static (StrategyImplementationBase instance, AssemblyLoadContext loadContext) GenerateStrategyInstance(
        byte[] assemblyBytes)
    {
        if (assemblyBytes is not { Length: > 0 })
            throw new ArgumentException("Assembly bytes cannot be null or empty.", nameof(assemblyBytes));

        var loadContext = new AssemblyLoadContext("StrategyAssemblyContext", isCollectible: true);
        Assembly assembly;

        using (var ms = new MemoryStream(assemblyBytes))
            assembly = loadContext.LoadFromStream(ms);

        var derivedType = assembly.GetTypes()
            .FirstOrDefault(t => t.IsSubclassOf(typeof(StrategyImplementationBase)) && !t.IsAbstract);

        if (derivedType == null)
            throw new InvalidOperationException("Aucune classe dérivée valide trouvée.");

        var instance = Activator.CreateInstance(derivedType) as StrategyImplementationBase 
            ?? throw new InvalidOperationException($"Impossible d'instancier {derivedType.FullName}.");

        return (instance, loadContext);
    }

    public static void UnloadStrategyInstance(StrategyImplementationBase? instance, AssemblyLoadContext? loadContext)
    {
        if (instance is IDisposable disposable)
            disposable.Dispose();

        if (loadContext != null)
        {
            loadContext.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private static (byte[] compiledAssembly, string? name, string? version) VerifyCompiledAssembly(
        byte[] assemblyBytes, List<Diagnostic> compileErrors)
    {
        var (instance, loadContext) = GenerateStrategyInstance(assemblyBytes);
        try
        {
            var derivedType = instance.GetType();
            string name = GetRequiredProperty(instance, "Name", "STRAT003", compileErrors);
            string version = GetRequiredProperty(instance, "Version", "STRAT004", compileErrors);
            ValidateMainChart(derivedType, compileErrors);

            if (compileErrors.Any(e => e.Severity == DiagnosticSeverity.Error))
                throw new CompilationException("La vérification de l'assembly a échoué.", compileErrors);

            return (assemblyBytes, name, version);
        }
        finally
        {
            UnloadStrategyInstance(instance, loadContext);
        }
    }

    private static string GetRequiredProperty(object instance, string propertyName, string errorCode, List<Diagnostic> errors)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
        {
            errors.Add(CreateDiagnostic(errorCode, $"La propriété '{propertyName}' est absente.", DiagnosticSeverity.Error));
            return string.Empty;
        }
        return property.GetValue(instance)?.ToString() ?? string.Empty;
    }

    private static void ValidateMainChart(Type derivedType, List<Diagnostic> compileErrors)
    {
        var hasMainChart = derivedType
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Any(f => typeof(IChart).IsAssignableFrom(f.FieldType) && f.GetCustomAttribute<MainChartAttribute>() != null);

        if (!hasMainChart)
            compileErrors.Add(CreateDiagnostic("STRAT002", $"Aucun 'MainChart' défini dans {derivedType.Name}.", DiagnosticSeverity.Error));
    }

    private static void PerformSecurityChecks(SyntaxTree syntaxTree, List<Diagnostic> compileErrors)
    {
        var disallowedNamespaces = new HashSet<string> { "Newtonsoft.Json", "System.Net.Http", "System.IO" };
        var disallowedTypes = new HashSet<string> { "System.Diagnostics.Process", "System.Reflection", "dynamic" };
        var fileSystemPrefixes = new HashSet<string> { "File", "Directory", "Path" };

        foreach (var node in syntaxTree.GetRoot().DescendantNodes())
        {
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax usingDirective)
            {
                var namespaceName = usingDirective.Name.ToString();
                if (disallowedNamespaces.Any(ns => namespaceName.StartsWith(ns)))
                    compileErrors.Add(CreateDiagnostic("SEC006", $"Le namespace '{namespaceName}' est interdit.", DiagnosticSeverity.Error));
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax identifierName)
            {
                var identifierText = identifierName.Identifier.Text;
                if (disallowedTypes.Contains(identifierText))
                    compileErrors.Add(CreateDiagnostic("SEC002", $"'{identifierText}' est interdit pour des raisons de sécurité.", DiagnosticSeverity.Error));
                else if (fileSystemPrefixes.Any(prefix => identifierText.StartsWith(prefix)))
                    compileErrors.Add(CreateDiagnostic("SEC005", $"L'utilisation de '{identifierText}' est restreinte.", DiagnosticSeverity.Warning));
            }
            else if (node is Microsoft.CodeAnalysis.CSharp.Syntax.UnsafeStatementSyntax)
            {
                compileErrors.Add(CreateDiagnostic("SEC003", "L'utilisation de 'unsafe' est interdite.", DiagnosticSeverity.Error));
            }
        }
    }

    private static Diagnostic CreateDiagnostic(string id, string message, DiagnosticSeverity severity)
    {
        return Diagnostic.Create(new DiagnosticDescriptor(id, "Vérification", message, "Sécurité", severity, true), Location.None);
    }
}
