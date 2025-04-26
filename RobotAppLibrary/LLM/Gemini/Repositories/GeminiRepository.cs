using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RobotAppLibrary.LLM.Gemini.Configuration;
using RobotAppLibrary.LLM.Gemini.Dao.Request;
using RobotAppLibrary.LLM.Gemini.Dao.Response;
using RobotAppLibrary.LLM.Model;

namespace RobotAppLibrary.LLM.Gemini.Repositories;

public class GeminiRepository(
    IHttpClientFactory httpClientFactory,
    ILogger<GeminiRepository> logger,
    IOptions<GeminiSettings> geminiSettings) : ILLMRepository
{
    private readonly GeminiSettings _geminiSettings = geminiSettings.Value;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Gemini");

    private string Url => $"v1beta/models/{_geminiSettings.ModelId}:generateContent?key={_geminiSettings.ApiKey}";

    public async Task<AnalyseMarcheResponse?> Analyse(string json)
    {
        string prompt = $@"
                         Contexte : Tu es un outil d'analyse de marchés financiers.
                         Ton rôle est d'analyser des données de trading pour déterminer le sentiment du marché
                         et identifier des niveaux de stop loss et de take profit.

                         Instructions pour l'IA :
                         1. Prends l'objet AnalyseMarcheRequest et désérialise la propriété DonneesAAnalyserJson en un objet exploitable.
                         2. Analyse les données de trading graphique et les indicateurs techniques présents dans l'objet désérialisé.
                         3. Sur la base de ton analyse, détermine un sentiment de marché sur une échelle de 0 à 100, où :
                            - 0 indique un fort sentiment de vente.
                           - 50 indique un sentiment neutre.
                            - 100 indique un fort sentiment d'achat.
                         4. Détermine des niveaux de Stop Loss et de Take Profit pertinents en fonction des données analysées.
                         5. Formate ta réponse **uniquement** comme un objet JSON sérialisé de type AnalyseMarcheResponse.
                         6. Ne retourne **rien d'autre** que cet objet JSON. Pas de texte avant, pas de texte après, pas d'explication supplémentaire.

                         Exemple de réponse attendue (en JSON sérialisé depuis AnalyseMarcheResponse) :
                        /*
                        {{
                          ""scoreSentiment"": 70,
                          ""stopLoss"": 101.0,
                          ""takeProfit"": 102.5
                        }}

                        ";

        var geminiRequest = new GeminiGenerationRequest
        {
            Contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    Parts = new List<GeminiPart> { new GeminiPart { Text = prompt } }
                }
            },
            GenerationConfig = new GenerationConfig()
            {
                ResponseMimeType = "application/json",
            }
        };

        try
        {
            var jsonRequest = JsonSerializer.Serialize(geminiRequest, new JsonSerializerOptions()
            {
            });
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(Url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                logger.LogError("Erreur de l'API Gemini ({StatusCode}): {ErrorBody}", response.StatusCode, errorBody);

                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiGenerationResponse>(jsonResponse);

            string? generatedText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                ?.Trim();

            if (string.IsNullOrWhiteSpace(generatedText))
            {
                logger.LogWarning("");
                return null;
            }

            logger.LogDebug("Texte brut retourné par Gemini pour keywords: {GeneratedText}", generatedText);

            try
            {
                if (generatedText.StartsWith("```json"))
                {
                    generatedText = generatedText.Replace("```json", "").TrimStart();
                }

                if (generatedText.StartsWith("```"))
                {
                    generatedText = generatedText.Substring(3);
                }

                if (generatedText.EndsWith("```"))
                {
                    generatedText = generatedText.Substring(0, generatedText.Length - 3);
                }

                generatedText = generatedText.Trim();


                AnalyseMarcheResponse? keywords = JsonSerializer.Deserialize<AnalyseMarcheResponse>(generatedText);
                return keywords ?? null;
            }
            catch (JsonException jsonEx)
            {
                logger.LogError(jsonEx,
                    "Impossible de parser la réponse de Gemini Réponse brute: {GeneratedText}",
                    generatedText);
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur inattendue lors de l'extraction de mots-clés via Gemini.");
            return null;
        }
    }
}