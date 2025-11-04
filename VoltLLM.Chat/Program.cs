using System;
using VoltLLM.Core;

using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;

namespace VoltLLM
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var options = Options.Parse(args);
            string[] exit_commands = ["/exit", "/bye", "/quit"];
            bool noColor = false;
            foreach (var arg in args)
            {
                if (arg.StartsWith("--no-color"))
                {
                    noColor = true;
                }
            }
            var convo = new LLMConversation(model: options.Model, token: options.Token, baseUrl: options.BaseUrl);
            Console.WriteLine($"Interactive chat with model: {options.Model} @ {options.BaseUrl} ({convo.client.ApiType})");
            Console.WriteLine("Type 'exit' or press Ctrl+C to quit.\n");

            while (true)
            {
                Console.Write("🧠 ");
                if (!noColor)
                    Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(options.Handle);
                if (!noColor)
                    Console.ResetColor();
                Console.Write(" > ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (exit_commands.Contains(input))
                    break;
                if (input.StartsWith("/models"))
                {
                    JObject? modelsObj = await convo.client.GetModelsAsync();
                    ModelManager.Show(modelsObj);
                    continue;
                }

                try
                {
                    var reply = await convo.SendWithFullContextAsync(input);

                    if (!string.IsNullOrWhiteSpace(reply))
                    {
                        Console.Write("🤖 ");
                        if (!noColor)
                            Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(options.Model);
                        if (!noColor)
                            Console.ResetColor();
                        Console.WriteLine($" > {reply}\n");
                    }
                    else
                    {
                        Console.WriteLine("[WARN] No response received.\n");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message}");
                }
            }

            Console.WriteLine("👋 Conversation ended.");
            return 0;
        }
    }
}

