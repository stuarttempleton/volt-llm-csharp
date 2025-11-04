# VoltLLM

VoltLLM is a C# library for interacting with Large Language Models (LLMs). The core functionality is provided by the `LLMClient` and `LLMConversation` classes, which enable developers to easily integrate LLM-powered chat and conversation features into their .NET applications.

## Features
- Simple API for sending and receiving messages to LLMs
- Conversation management via `LLMConversation`
- Extensible client interface (`LLMClient`)
- Example chat application in `VoltLLM.Chat`

## Getting Started

### Prerequisites
- .NET 9.0 SDK or later

### Building
Clone the repository and build using the .NET CLI:

```powershell
# Clone the repository
git clone https://github.com/stuarttempleton/volt-llm-csharp.git
cd volt-llm-csharp

# Build the solution
dotnet build VoltLLM.sln
```

### Usage
Reference the `VoltLLM.Core` project in your .NET application. See the `LLMClient` and `LLMConversation` classes for API details.

#### Example
```csharp
using VoltLLM.Core;

var client = new LLMClient(/* config */);
var conversation = new LLMConversation(client);
conversation.SendMessage("Hello, LLM!");
var response = conversation.GetLatestResponse();
Console.WriteLine(response);
```

## Project Structure
- `VoltLLM.Core/` - Core library with LLM client and conversation logic
- `VoltLLM.Chat/` - Example chat application using the core library

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! Please open issues or submit pull requests for improvements or bug fixes.

## Author
Stuart Templeton
