using System.Reflection;
using System.Runtime.Loader;

namespace RobotAppLibrary.StrategyDynamicCompiler;

public class CustomLoadContext() : AssemblyLoadContext(true)
{
    protected override Assembly Load(AssemblyName assemblyName)
    {
        return null;
    }
}