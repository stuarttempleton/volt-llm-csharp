using Newtonsoft.Json.Linq;
using System;

public static class ModelManager
{
    public static void Show(JObject? m)
    {
        if (m == null)
        {
            Console.WriteLine("Feature not available.");
            return;
        }

        var modelList = new System.Text.StringBuilder();
        bool found = false;

        try
        {
            if (m["data"] is JArray dataArray) // OpenWebUI
            {
                foreach (var server_model in dataArray)
                {
                    string name = server_model["name"]?.ToString() ?? "??";
                    string id = server_model["id"]?.ToString() ?? "??";
                    modelList.AppendLine($"\t{name} - {id}");
                }
                found = true;
            }
            else if (m["models"] is JArray ollamaArray) // Ollama
            {
                foreach (var server_model in ollamaArray)
                {
                    string name = server_model["name"]?.ToString() ?? "??";
                    string modelId = server_model["model"]?.ToString() ?? "??";
                    modelList.AppendLine($"\t{name} - {modelId}");
                }
                found = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing models: {ex.Message}");
        }

        if (found)
        {
            Console.WriteLine("Available Models:");
            Console.WriteLine(modelList.ToString());
        }
        else
        {
            Console.WriteLine("Feature not available.");
        }
    }
}
