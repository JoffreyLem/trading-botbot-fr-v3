using Microsoft.CodeAnalysis;

namespace RobotAppLibrary.StrategyDynamicCompiler;

public class CompilationException(string message, IEnumerable<Diagnostic> compileErrors) : Exception(message)
{
    public IEnumerable<Diagnostic> CompileErrors { get; } = compileErrors;
}