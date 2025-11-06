using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace VoltLLM.Core
{

    /// <summary>
    /// Provides methods to interact with various LLM APIs, including model detection and prompt handling.
    /// </summary>
    public class LLMClient
    {
        private readonly string baseUrl;
        private readonly string model;
        private readonly float temperature;
        private readonly string bearerToken;
        private readonly HttpClient client;
        /// <summary>
        /// Gets the detected API type (e.g., "ollama", "openwebui", or "unknown").
        /// </summary>
        public string ApiType { get; private set; } = "unknown";
        /// <summary>
        /// Gets the dictionary of API endpoints for models and chat operations.
        /// </summary>
        public Dictionary<string, string> Endpoints { get; private set; } = new Dictionary<string, string>();
        internal async Task DetectApiTypeAsync()
        {
            var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {bearerToken}" },
            { "Content-Type", "application/json" }
        };

            // Try Ollama first (no bearer token required)
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/tags");
                var response = await client.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ApiType = "ollama";
                    Endpoints = new Dictionary<string, string>
                {
                    { "models", $"{baseUrl}/api/tags" },
                    { "chat", $"{baseUrl}/api/chat" }
                };
                    return;
                }
            }
            catch { }

            // Try Open WebUI (requires bearer token)
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/models");
                foreach (var h in headers)
                    request.Headers.TryAddWithoutValidation(h.Key, h.Value);
                var response = await client.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JObject.Parse(json);
                    if ((result["data"] != null) || (result["choices"] != null))
                    {
                        ApiType = "openwebui";
                        Endpoints = new Dictionary<string, string>
                    {
                        { "models", $"{baseUrl}/api/models" },
                        { "chat", $"{baseUrl}/api/chat/completions" }
                    };
                        if (string.IsNullOrEmpty(bearerToken))
                        {
                            Logger.LogWarning("Missing API token. Please set LLM_API_TOKEN as a user environment variable.");
                            Logger.LogInformation("\nNeed a hand?");
                            Logger.LogInformation("PowerShell (permanent):");
                            Logger.LogInformation("  [System.Environment]::SetEnvironmentVariable(\"LLM_API_TOKEN\", \"your-secret-token-here\", \"User\")");
                            Logger.LogInformation("PowerShell (temporary):");
                            Logger.LogInformation("  $env:LLM_API_TOKEN = \"your-secret-token-here\"");
                        }
                        return;
                    }
                }
            }
            catch { }

            // Unknown API
            ApiType = "unknown";
            Endpoints = new Dictionary<string, string>
        {
            { "models", $"{baseUrl}/api/models" },
            { "chat", $"{baseUrl}/api/chat/completions" }
        };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LLMClient"/> class with the specified base URL, API token, model name, and temperature.
        /// </summary>
        /// <param name="baseUrl">The base URL of the LLM API.</param>
        /// <param name="token">The API bearer token (optional).</param>
        /// <param name="model">The model name to use (default is "Gemma3").</param>
        /// <param name="temperature">The temperature setting for responses (default is 0.2).</param>
    public LLMClient(string baseUrl = "http://localhost:11434/", string token = "", string model = "Gemma3", float temperature = 0.2f)
    {
    this.baseUrl = baseUrl.TrimEnd('/');
    this.model = model;
    this.temperature = temperature;
    this.bearerToken = string.IsNullOrEmpty(token) ? (Environment.GetEnvironmentVariable("LLM_API_TOKEN") ?? "") : token;
    this.client = new HttpClient();
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
    // Initialization moved to InitAsync() for Unity
#else
    // Synchronous initialization for non-Unity environments
    DetectApiTypeAsync().Wait();
#endif
    }

    /// <summary>
    /// Asynchronously initializes the API type detection for the LLM client.
    /// </summary>
    public async Task InitAsync()
    {
        await DetectApiTypeAsync();
    }

        /// <summary>
        /// Retrieves the available models from the LLM API.
        /// </summary>
        /// <returns>A <see cref="JObject"/> containing the models, or null if the request fails.</returns>
    public async Task<JObject> GetModelsAsync()
        {
            try
            {
                var modelsEndpoint = Endpoints.TryGetValue("models", out string value) ? value : $"{baseUrl}/api/models";
                var request = new HttpRequestMessage(HttpMethod.Get, modelsEndpoint);
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");
                request.Headers.Add("Accept", "application/json");

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Request failed: {ex.Message}");
                return new JObject();
            }
        }

        /// <summary>
        /// Sends a prompt to the LLM API with an optional system prompt and returns the response content.
        /// </summary>
        /// <param name="prompt">The user prompt to send.</param>
        /// <param name="systemPrompt">An optional system prompt to set the assistant's behavior.</param>
        /// <returns>The response content from the LLM API, or null if the request fails.</returns>
    public async Task<string> SendPromptAsync(string prompt, string systemPrompt = "")
        {
            var messages = new[]
            {
                new Dictionary<string, string> { ["role"] = "system", ["content"] = string.IsNullOrEmpty(systemPrompt) ? "You are a senior application security engineer." : systemPrompt },
                new Dictionary<string, string> { ["role"] = "user", ["content"] = prompt }
            };

            var payload = new
            {
                model = this.model,
                messages = messages,
                temperature = this.temperature,
                stream = false
            };

            var chatEndpoint = Endpoints.TryGetValue("chat", out string value) ? value : $"{baseUrl}/api/chat/completions";
            return await PostAndExtractContentAsync(chatEndpoint, payload);
        }

        /// <summary>
        /// Sends a conversation (list of messages) to the LLM API and returns the response content.
        /// </summary>
        /// <param name="messages">A list of message dictionaries representing the conversation history.</param>
        /// <returns>The response content from the LLM API, or null if the request fails.</returns>
    public async Task<string> SendConversationAsync(List<Dictionary<string, string>> messages)
        {
            var payload = new
            {
                model = this.model,
                messages = messages,
                temperature = this.temperature,
                stream = false
            };

            var chatEndpoint = Endpoints.TryGetValue("chat", out string value) ? value : $"{baseUrl}/api/chat/completions";
            return await PostAndExtractContentAsync(chatEndpoint, payload);
        }

    private async Task<string> PostAndExtractContentAsync(string url, object payload)
        {
            try
            {
                var jsonPayload = JsonConvert.SerializeObject(payload);
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(json);
                return ExtractContent(result);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Request failed: {ex.Message}");
                return "";
            }
        }

        private string ExtractContent(JObject result)
        {
            if (result["choices"] is JArray choices && choices.Count > 0)
            {
                return choices[0]["message"]?["content"]?.ToString() ?? String.Empty;
            }

            if (result["message"] != null)
            {
                return result["message"]?["content"]?.ToString() ?? String.Empty;
            }

            Logger.LogWarning("Unexpected JSON structure.");
            return String.Empty;
        }
    }
}
