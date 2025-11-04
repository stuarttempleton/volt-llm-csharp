using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace VoltLLM.Core;

/// <summary>
/// Represents a conversation with a language model, managing messages and interactions.
/// </summary>
public class LLMConversation
{
    /// <summary>
    /// Gets the LLM client used for communication with the language model.
    /// </summary>
    public LLMClient client { get; private set; }
    internal readonly List<Dictionary<string, string>> messages;

    /// <summary>
    /// Initializes a new instance of the <see cref="LLMConversation"/> class.
    /// </summary>
    /// <param name="model">The model to use for the conversation.</param>
    /// <param name="systemPrompt">The system prompt to initialize the conversation.</param>
    /// <param name="token">The authentication token for the LLM client.</param>
    /// <param name="baseUrl">The base URL of the LLM service.</param>
        public LLMConversation(string model = "gemma3", string? systemPrompt = null, string? token = null, string baseUrl = "http://localhost:11434/")
        {
            client = new LLMClient(baseUrl: baseUrl, token: token, model: model);
            messages = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "role", "system" },
                    { "content", systemPrompt ?? "You are a helpful and knowledgeable AI assistant. Provide clear, concise, and accurate responses. When appropriate, ask clarifying questions or provide examples." }
                }
            };
        }

    /// <summary>
    /// Sends a message to the language model and receives a response.
    /// </summary>
    /// <param name="userContent">The user's message content.</param>
    /// <param name="sendEverything">If true, sends the full conversation context.</param>
    /// <param name="assistantOnly">If true, sends only system and assistant messages as context.</param>
    /// <returns>The assistant's reply as a string, or null if no response.</returns>
    public async Task<string?> SendAsync(string userContent, bool sendEverything = false, bool assistantOnly = false)
    {
        List<Dictionary<string, string>> prompt;

        if (assistantOnly)
        {
            // system + all assistant messages
            prompt = new List<Dictionary<string, string>> { messages[0] };
            prompt.AddRange(messages.FindAll(m => m["role"] == "assistant"));
            prompt.Add(new Dictionary<string, string> { { "role", "user" }, { "content", userContent } });
        }
        else if (sendEverything)
        {
            prompt = new List<Dictionary<string, string>>(messages);
            prompt.Add(new Dictionary<string, string> { { "role", "user" }, { "content", userContent } });
        }
        else
        {
            prompt = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string> { { "role", "system" }, { "content", messages[0]["content"] } },
                new Dictionary<string, string> { { "role", "user" }, { "content", userContent } }
            };
        }

        var reply = await client.SendConversationAsync(prompt);

        messages.Add(new Dictionary<string, string> { { "role", "user" }, { "content", userContent } });
        messages.Add(new Dictionary<string, string> { { "role", "assistant" }, { "content", reply ?? "[NO RESPONSE]" } });

        return reply;
    }

    /// <summary>
    /// Sends a message to the language model with the full conversation context.
    /// </summary>
    /// <param name="userContent">The user's message content.</param>
    /// <returns>The assistant's reply as a string, or null if no response.</returns>
    public Task<string?> SendWithFullContextAsync(string userContent)
    {
        return SendAsync(userContent, sendEverything: true);
    }

    /// <summary>
    /// Sends a message to the language model with only system and assistant messages as context.
    /// </summary>
    /// <param name="userContent">The user's message content.</param>
    /// <returns>The assistant's reply as a string, or null if no response.</returns>
    public Task<string?> SendWithSummaryContextAsync(string userContent)
    {
        return SendAsync(userContent, assistantOnly: true);
    }

    /// <summary>
    /// Saves the current conversation transcript to a file in JSON format.
    /// </summary>
    /// <param name="path">The file path where the transcript will be saved.</param>
    public async Task SaveTranscriptAsync(string path)
    {
    var json = JsonConvert.SerializeObject(messages, Formatting.Indented);
    await File.WriteAllTextAsync(path, json);
    }

    /// <summary>
    /// Loads a conversation transcript from a file in JSON format and replaces the current messages.
    /// </summary>
    /// <param name="path">The file path from which the transcript will be loaded.</param>
    public async Task LoadTranscriptAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        var loaded = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);
        if (loaded != null)
        {
            messages.Clear();
            messages.AddRange(loaded);
        }
    }
}