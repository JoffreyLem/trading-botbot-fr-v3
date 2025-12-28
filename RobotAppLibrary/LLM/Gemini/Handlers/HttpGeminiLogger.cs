using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;

namespace RobotAppLibrary.LLM.Gemini.Handlers
{
    public class HttpGeminiLogger(ILogger<HttpGeminiLogger> logger) : IHttpClientLogger
    {
        private readonly ILogger<HttpGeminiLogger> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private const string ApiKeyName = "key";

        private string GetSanitizedUriString(HttpRequestMessage request)
        {
            if (request.RequestUri == null) return "(URI non disponible)";
            return RemoveQueryStringByKey(request.RequestUri.OriginalString, ApiKeyName);
        }

        public object? LogRequestStart(HttpRequestMessage request)
        {
            string sanitizedUri = GetSanitizedUriString(request);
            _logger.LogInformation("➡️ HTTP {Method} {Uri} - Sending request",
                request.Method,
                sanitizedUri); 
            return null;
        }

        public void LogRequestStop(object? context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
        {
            string sanitizedUri = GetSanitizedUriString(request);
            _logger.LogInformation("✅ HTTP {Method} {Uri} completed in {Elapsed}ms with status {StatusCode}",
                request.Method,
                sanitizedUri,
                elapsed.TotalMilliseconds,
                (int)response.StatusCode);
        }

        public void LogRequestFailed(
            object? context,
            HttpRequestMessage request,
            HttpResponseMessage? response,
            Exception exception,
            TimeSpan elapsed)
        {
            string sanitizedUri = GetSanitizedUriString(request);
            _logger.LogError(exception,
                "❌ HTTP {Method} {Uri} failed after {Elapsed}ms. Status: {StatusCode}",
                request.Method,
                sanitizedUri,
                elapsed.TotalMilliseconds,
                response?.StatusCode.ToString() ?? "No response");
        }
        
                  public  string RemoveQueryStringByKey(string? uriString, string keyToRemove)
        {
            if (string.IsNullOrEmpty(uriString) || string.IsNullOrEmpty(keyToRemove))
                return uriString ?? string.Empty; // Gérer null

            bool likelyContainsKey = uriString.Contains("?" + keyToRemove + "=", StringComparison.OrdinalIgnoreCase) ||
                                     uriString.Contains("&" + keyToRemove + "=", StringComparison.OrdinalIgnoreCase);

            if (!uriString.Contains('?') || !likelyContainsKey)
            {
                return uriString;
            }

            try
            {
                var uri = new Uri(uriString);
                var baseUri = uri.GetLeftPart(UriPartial.Path);
                var fragment = uri.Fragment;
                var queryString = uri.Query.Length > 0 ? uri.Query.Substring(1) : string.Empty;

                if (string.IsNullOrEmpty(queryString)) return uriString;

                var queryParameters = QueryHelpers.ParseQuery(queryString);

                var filteredParameters = queryParameters
                    .Where(kvp => !kvp.Key.Equals(keyToRemove, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!filteredParameters.Any())
                {
                    return baseUri + fragment; 
                }

                var sb = new StringBuilder();
                foreach (var kvp in filteredParameters)
                {
                    foreach (var value in kvp.Value)
                    {
                        if (sb.Length > 0) sb.Append('&');
                        sb.Append(UrlEncoder.Default.Encode(kvp.Key));
                        sb.Append('=');
                        if (value != null) sb.Append(UrlEncoder.Default.Encode(value));
                    }
                }

                var uriBuilder = new UriBuilder(uri) { Query = sb.ToString(), Fragment = "" };

                 return uriBuilder.Uri.GetLeftPart(UriPartial.Path) + uriBuilder.Uri.Query + fragment;


            }
            catch (UriFormatException ex)
            {
                Console.WriteLine($"URI Invalide lors de la suppression de la clé '{keyToRemove}': {ex.Message}"); 
                return uriString; 
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Erreur inattendue lors de la suppression de la clé '{keyToRemove}': {ex.Message}");
                 return uriString;
            }
        }
    }
    

}