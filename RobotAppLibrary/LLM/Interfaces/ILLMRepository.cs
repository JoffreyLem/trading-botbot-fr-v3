using RobotAppLibrary.LLM.Model;

namespace RobotAppLibrary.LLM.Interfaces;

public interface ILLMRepository
{
    Task<AnalyseMarcheResponse?> Analyse(AnalyseMarcheRequest analyseMarcheRequest);
}