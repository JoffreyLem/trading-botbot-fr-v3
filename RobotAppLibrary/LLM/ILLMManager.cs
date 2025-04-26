namespace RobotAppLibrary.LLM;

public interface ILLMManager
{
    ILLMRepository GetLLM(Model.LLM llm);
}