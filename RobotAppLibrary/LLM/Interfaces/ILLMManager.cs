namespace RobotAppLibrary.LLM.Interfaces;

public interface ILLMManager
{
    ILLMRepository GetLLM(Model.LLM llm);
}