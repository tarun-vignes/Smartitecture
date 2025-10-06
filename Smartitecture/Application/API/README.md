# AIPal API Documentation

This document provides an overview of the API components added to the AIPal Windows desktop application to enable external applications to interact with AIPal's AI assistant capabilities.

## Overview

The AIPal API is a RESTful API that runs alongside the WinUI desktop application. It provides endpoints for:

- Sending chat messages to the AI assistant
- Processing natural language commands
- Executing system commands through the AI assistant

## API Endpoints

### POST /api/chat

Process a chat message, optionally parsing it as a command.

#### Request

```json
{
  "message": "Open Notepad",
  "parseAsCommand": true
}
```

Parameters:
- `message` (string, required): The user's message or command
- `parseAsCommand` (boolean, optional, default: true): Whether to attempt to parse the message as a system command

#### Response

```json
{
  "response": "Successfully executed LaunchApp command.",
  "commandExecuted": true,
  "commandName": "LaunchApp",
  "commandSuccess": true
}
```

Fields:
- `response` (string): The AI assistant's response message
- `commandExecuted` (boolean): Whether a command was executed
- `commandName` (string): The name of the command that was executed, if any
- `commandSuccess` (boolean): Whether the command execution was successful

## Architecture

The API implementation consists of the following components:

1. **API Models**: Data transfer objects (DTOs) for API requests and responses
   - `ChatRequestDto`: Represents a chat request from an external application
   - `ChatResponseDto`: Represents a response to a chat request

2. **API Controller**: Handles HTTP requests and responses
   - `ChatController`: Processes chat messages and commands

3. **API Services**: Business logic for the API
   - `IApiService`: Interface for the API service
   - `ApiService`: Implementation of the API service

4. **API Host**: Manages the web server that hosts the API
   - `ApiHostService`: Hosts the API within the WinUI desktop application

## Integration with Existing Components

The API components integrate with the existing AIPal components:

- Uses the `ILLMService` for natural language processing and command parsing
- Uses the `CommandMapper` for executing system commands
- Runs alongside the WinUI desktop application in the same process

## Usage Examples

### Using cURL

```bash
curl -X POST "http://localhost:5000/api/chat" \
     -H "Content-Type: application/json" \
     -d "{\"message\":\"What time is it?\",\"parseAsCommand\":false}"
```

### Using C#

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public async Task<string> SendChatMessage(string message, bool parseAsCommand = true)
{
    using var httpClient = new HttpClient();
    var request = new
    {
        message = message,
        parseAsCommand = parseAsCommand
    };
    
    var content = new StringContent(
        JsonSerializer.Serialize(request),
        Encoding.UTF8,
        "application/json");
        
    var response = await httpClient.PostAsync("http://localhost:5000/api/chat", content);
    response.EnsureSuccessStatusCode();
    
    var responseBody = await response.Content.ReadAsStringAsync();
    return responseBody;
}
```

## Security Considerations

- The API is currently configured to run on localhost only
- CORS is enabled to allow web applications to access the API
- No authentication is currently implemented; consider adding authentication for production use
