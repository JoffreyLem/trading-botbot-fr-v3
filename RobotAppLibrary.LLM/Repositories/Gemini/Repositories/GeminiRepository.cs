using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RobotAppLibrary.LLM.Repositories.Gemini.Configuration;
using RobotAppLibrary.LLM.Repositories.Gemini.Dao.Request;
using RobotAppLibrary.LLM.Repositories.Gemini.Dao.Response;
using RobotAppLibrary.LLM.Repositories.Models;

namespace RobotAppLibrary.LLM.Repositories.Gemini.Repositories;

public class GeminiRepository(
    IHttpClientFactory httpClientFactory,
    ILogger<GeminiRepository> logger,
    IOptions<GeminiSettings> geminiSettings) : ILLMModel
{
    private readonly GeminiSettings _geminiSettings = geminiSettings.Value;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    private string Url => $"v1beta/models/{_geminiSettings.ModelId}:generateContent?key={_geminiSettings.ApiKey}";

    public async Task<List<string>> ExtractKeywordsAsync(string prompt)
    {
        var keywordExtractionPrompt =
            $"""
             CONTEXTE : Tu es un outil d'extraction de mots-clés.

             ENTRÉE UTILISATEUR (peut contenir une question ou plusieurs mots-clés) : 
             '{prompt}'

             TA MISSION :
             1. Analyse l' `ENTRÉE UTILISATEUR`.
             2. Identifie et extrais **uniquement** les mots ou groupes de mots qui sont les plus **importants et pertinents** pour effectuer une recherche d'information (juridique, administrative ou générale). Ignore les mots vides (articles, prépositions...).
             3. Mets tous les mots-clés extraits en **minuscules**.
             4. Assure-toi qu'il n'y a **aucun doublon** dans la liste finale.
             5. Formate le résultat **exclusivement** comme une **liste JSON de chaînes de caractères**.
             6. Ne retourne **rien d'autre** que cette liste JSON. Pas de texte avant, pas de texte après, pas d'explication.

             Exemple de réponse attendue pour "Quel est le délai de préavis pour une démission de cadre avec la convention Syntec ?":
             ["délai", "préavis", "démission", "cadre", "convention", "syntec"]

             Autre exemple pour "pomme voisin arbre":
             ["pomme", "voisin", "arbre"]

             Autre exemple pour "prescription facture edf":
             ["prescription", "facture", "edf"]
             """;

        var geminiRequest = new GeminiGenerationRequest
        {
            Contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    Parts = new List<GeminiPart> { new GeminiPart { Text = keywordExtractionPrompt } }
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

                return new List<string>();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiGenerationResponse>(jsonResponse);

            string? generatedText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                ?.Trim();

            if (string.IsNullOrWhiteSpace(generatedText))
            {
                logger.LogWarning(
                    "L'API Gemini n'a retourné aucun texte pour l'extraction de mots-clés. Prompt: {Prompt}", prompt);
                return new List<string>();
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


                List<string>? keywords = JsonSerializer.Deserialize<List<string>>(generatedText);
                return keywords ?? new List<string>();
            }
            catch (JsonException jsonEx)
            {
                logger.LogError(jsonEx,
                    "Impossible de parser la réponse de Gemini comme une liste JSON. Réponse brute: {GeneratedText}",
                    generatedText);
                return new List<string>();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur inattendue lors de l'extraction de mots-clés via Gemini.");
            return new List<string>();
        }
    }


}