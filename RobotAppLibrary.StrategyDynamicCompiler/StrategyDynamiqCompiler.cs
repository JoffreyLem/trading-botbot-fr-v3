using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyModel;
using RobotAppLibrary.Chart;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles.Attribute;
using RobotAppLibrary.Strategy;

namespace RobotAppLibrary.StrategyDynamicCompiler;

public static class StrategyDynamiqCompiler
{
    public static (byte[] compiledAssembly, string? name, string? version) TryCompileSourceCode(string sourceCode,
        bool verifyAssembly = true)
    {
        try
        {
            var compileErrors = new List<Diagnostic>();

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            PerformSecurityChecks(syntaxTree, compileErrors);

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

            if (compileResult.Success)
            {
                var dataToReturn = ms.ToArray();

                if (verifyAssembly)
                {
                    VerifyAssembly(dataToReturn, compileErrors, out var nameValue, out var versionValue);

                    if (compileErrors.Any())
                    {
                        throw new CompilationException("La vérification de l'assembly a échoué.",
                            compileErrors.Where(error => error.Severity == DiagnosticSeverity.Error));
                    }

                    return (dataToReturn, nameValue, versionValue);
                }

                return (dataToReturn, null, null);
            }

            compileErrors.AddRange(compileResult.Diagnostics);

            throw new CompilationException("La compilation a échoué.",
                compileErrors.Where(error => error.Severity == DiagnosticSeverity.Error));
        }
        finally
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public static (StrategyImplementationBase instance, AssemblyLoadContext loadContext) GenerateStrategyInstance(
        byte[] assemblyBytes)
    {
        if (assemblyBytes == null || assemblyBytes.Length == 0)
            throw new ArgumentException("Assembly bytes cannot be null or empty.", nameof(assemblyBytes));

        var assemblyLoadContext = new AssemblyLoadContext("StrategyAssemblyContext", isCollectible: true);

        Assembly assembly;
        using (var ms = new MemoryStream(assemblyBytes))
        {
            assembly = assemblyLoadContext.LoadFromStream(ms);
        }

        var derivedType = assembly.GetTypes().FirstOrDefault(t =>
            t.IsSubclassOf(typeof(StrategyImplementationBase)) && !t.IsAbstract);

        if (derivedType == null)
        {
            assemblyLoadContext.Unload();
            throw new InvalidOperationException(
                "No non-abstract derived type of StrategyImplementationBase found in the assembly.");
        }

        var instance = Activator.CreateInstance(derivedType) as StrategyImplementationBase;

        if (instance == null)
        {
            assemblyLoadContext.Unload();
            throw new InvalidOperationException(
                $"Failed to create an instance of the derived type {derivedType.FullName}.");
        }

        return (instance, assemblyLoadContext);
    }

    public static void UnloadStrategyInstance(StrategyImplementationBase? instance, AssemblyLoadContext? loadContext)
    {
        if (instance != null)
        {
            if (instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        if (loadContext != null)
        {
            loadContext.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }


    private static void VerifyAssembly(
        byte[] assemblyBytes,
        List<Diagnostic> compileErrors,
        out string nameValue,
        out string versionValue)
    {
        var strategy = GenerateStrategyInstance(assemblyBytes);

        try
        {
            nameValue = "";
            versionValue = "";

            var derivedType = strategy.instance.GetType();

            if ((!derivedType.IsSubclassOf(typeof(StrategyImplementationBase))))
            {
                compileErrors.Add(Diagnostic.Create(new DiagnosticDescriptor(
                    "STRAT001", "Vérification de l'assembly", "No derived types of StrategyImplementationBase found.",
                    "Vérification", DiagnosticSeverity.Error, true), Location.None));
                return;
            }

            var chartFields = derivedType
                .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(f => typeof(IChart).IsAssignableFrom(f.FieldType)).ToList();

            if (chartFields.All(f => f.GetCustomAttribute<MainChartAttribute>() == null))
            {
                compileErrors.Add(Diagnostic.Create(new DiagnosticDescriptor(
                    "STRAT002", "Vérification de l'assembly", $"No main chart defined in class {derivedType.Name}.",
                    "Vérification", DiagnosticSeverity.Error, true), Location.None));
            }

            var nameProperty = derivedType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            var versionProperty = derivedType.GetProperty("Version", BindingFlags.Public | BindingFlags.Instance);

            if (nameProperty is null)
            {
                compileErrors.Add(Diagnostic.Create(new DiagnosticDescriptor(
                    "STRAT003", "Vérification de l'assembly",
                    $"Property 'Name' is missing in class {derivedType.Name}.",
                    "Vérification", DiagnosticSeverity.Error, true), Location.None));
            }
            else
            {
                nameValue = strategy.instance.Name;
            }

            if (versionProperty is null)
            {
                compileErrors.Add(Diagnostic.Create(new DiagnosticDescriptor(
                    "STRAT004", "Vérification de l'assembly",
                    $"Property 'Version' is missing in class {derivedType.Name}.",
                    "Vérification", DiagnosticSeverity.Error, true), Location.None));
            }
            else
            {
                versionValue = strategy.instance.Version;
            }
        }
        finally
        {
            UnloadStrategyInstance(strategy.instance, strategy.loadContext);
        }
    }

    private static void PerformSecurityChecks(SyntaxTree syntaxTree, List<Diagnostic> compileErrors)
    {
        var root = syntaxTree.GetRoot();

        // Liste pour les types et namespaces interdits
        var disallowedTypes = new HashSet<string> { "System.Diagnostics.Process", "System.Reflection", "dynamic" };
        var disallowedNamespaces = new HashSet<string> { "Newtonsoft.Json", "System.Net.Http", "System.IO" };
        var fileSystemPrefixes = new HashSet<string> { "File", "Directory", "Path" };

        // Parcourir l'arbre de syntaxe une seule fois
        foreach (var node in root.DescendantNodes())
        {
            // Vérifier les directives 'using' pour les namespaces interdits
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax usingDirective)
            {
                var namespaceName = usingDirective.Name.ToString();
                if (disallowedNamespaces.Any(ns => namespaceName.StartsWith(ns)))
                {
                    compileErrors.Add(Diagnostic.Create(new DiagnosticDescriptor(
                            "SEC006",
                            "Vérification de sécurité",
                            $"L'utilisation du namespace '{namespaceName}' est interdite.",
                            "Sécurité",
                            DiagnosticSeverity.Error,
                            true),
                        usingDirective.GetLocation()));
                }
            }

            // Vérifier l'utilisation d'identifiants pour des types dangereux
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax identifierName)
            {
                var identifierText = identifierName.Identifier.Text;

                if (disallowedTypes.Contains(identifierText))
                {
                    compileErrors.Add(Diagnostic.Create(new DiagnosticDescriptor(
                            "SEC002",
                            "Vérification de sécurité",
                            $"L'utilisation de '{identifierText}' est interdite pour des raisons de sécurité.",
                            "Sécurité",
                            DiagnosticSeverity.Error,
                            true),
                        identifierName.GetLocation()));
                }

                if (fileSystemPrefixes.Any(prefix => identifierText.StartsWith(prefix)))
                {
                    compileErrors.Add(Diagnostic.Create(new DiagnosticDescriptor(
                            "SEC005",
                            "Vérification de sécurité",
                            $"L'utilisation de '{identifierText}' est restreinte pour des raisons de sécurité.",
                            "Sécurité",
                            DiagnosticSeverity.Warning,
                            true),
                        identifierName.GetLocation()));
                }
            }

            // Vérifier l'utilisation du mot clé 'unsafe'
            if (node is Microsoft.CodeAnalysis.CSharp.Syntax.UnsafeStatementSyntax)
            {
                compileErrors.Add(Diagnostic.Create(new DiagnosticDescriptor(
                        "SEC003",
                        "Vérification de sécurité",
                        "L'utilisation de 'unsafe' est interdite pour des raisons de sécurité.",
                        "Sécurité",
                        DiagnosticSeverity.Error,
                        true),
                    node.GetLocation()));
            }
        }
    }
}