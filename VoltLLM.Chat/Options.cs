using System;

public class Options
{
    public string Model { get; set; } = "Gemma3:1b";
    public string Handle { get; set; } = "You";
    public string BaseUrl { get; set; } = "http://localhost:11434/";
    public string? Token { get; set; } = null;

    public static Options Parse(string[] args)
    {
        var options = new Options();
        foreach (var arg in args)
        {
            if (arg.StartsWith("--model="))
            {
                options.Model = arg.Substring("--model=".Length);
            }
            else if (arg.StartsWith("--handle="))
            {
                options.Handle = arg.Substring("--handle=".Length);
            }
            else if (arg.StartsWith("--base-url="))
            {
                options.BaseUrl = arg.Substring("--base-url=".Length);
            }
        }
        options.Token = Environment.GetEnvironmentVariable("LLM_API_TOKEN");
        return options;
    }
}
