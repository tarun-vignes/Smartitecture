# AIPal Agent System Documentation

## Overview

The AIPal Agent System extends the existing AI assistant with more powerful agent capabilities inspired by OpenAI's agent framework. Unlike the simpler query and chat interfaces, agents can use tools, perform multi-step reasoning, and autonomously complete complex tasks.

## Key Components

### 1. Agent Service

The `IAgentService` provides the core agent functionality:

- **Process requests**: Handles user requests through an agent that can use tools
- **Tool registration**: Register tools that the agent can use to perform actions
- **Multi-step reasoning**: Allows the agent to break down complex tasks into steps

### 2. Agent Tools

Tools are functions that the agent can use to interact with the system and external services:

- **System Tools**: Access system information, launch applications, search files, etc.
- **Web Tools**: Fetch web content, search the web, get weather information, etc.
- **Custom Tools**: Extend with your own domain-specific tools

### 3. API Endpoints

The `AgentController` exposes these endpoints:

- **POST /api/agent/process**: Process a request through the agent
- **GET /api/agent/tools**: Get information about available tools

## How Agents Work

1. **Request Processing**:
   - User sends a request to the agent
   - Agent analyzes the request and determines required actions
   - Agent may use one or more tools to complete the task
   - Agent provides a final response with results

2. **Tool Usage**:
   - Agent decides which tool to use based on the task
   - Agent provides necessary parameters to the tool
   - Tool executes and returns results
   - Agent may use multiple tools in sequence to complete complex tasks

3. **Context Management**:
   - Agent maintains conversation context using a `contextId`
   - Previous interactions inform the agent's understanding
   - Follow-up requests can reference previous information

## Agent vs. Query System

The Agent System complements the existing Query System:

| Feature | Query System | Agent System |
|---------|-------------|--------------|
| **Complexity** | Simple, structured queries | Complex, multi-step tasks |
| **Autonomy** | Limited, predefined actions | High, can reason and choose tools |
| **Tools** | Fixed command set | Extensible tool ecosystem |
| **Use Case** | Quick commands, simple questions | Complex tasks, research, troubleshooting |

## Available Tools

### System Tools

- **LaunchApplication**: Launches a Windows application
- **GetSystemInfo**: Gets basic information about the system
- **SearchFiles**: Searches for files matching a pattern
- **GetDateTime**: Gets current date and time information

### Web Tools

- **FetchWebContent**: Fetches content from a web URL
- **GetWeatherInfo**: Gets weather information for a location
- **SearchWeb**: Searches the web for information

## Integration with Existing Components

The Agent System integrates with:

- **ILLMService**: For natural language understanding
- **CommandMapper**: For executing system commands
- **Query API**: Complements the structured query interface

## Usage Examples

### Processing a Request

```csharp
// Create a request
var request = new AgentRequestDto
{
    Message = "Find all PDF files in my Documents folder and tell me which one was modified most recently",
    ContextId = "session123"
};

// Process the request
var response = await httpClient.PostAsJsonAsync("api/agent/process", request);
var result = await response.Content.ReadFromJsonAsync<AgentResponseDto>();

// Display the result
Console.WriteLine(result.Response);

// View actions taken
foreach (var action in result.Actions)
{
    Console.WriteLine($"Tool: {action.ToolName}");
    Console.WriteLine($"Result: {action.Success}");
}
```

### Following Up

```csharp
// Follow-up request using the same context ID
var followUpRequest = new AgentRequestDto
{
    Message = "Open that file for me",
    ContextId = "session123" // Same as previous request
};

// Process the follow-up
var followUpResponse = await httpClient.PostAsJsonAsync("api/agent/process", followUpRequest);
```

## Configuration

Configure the Agent System in your `appsettings.json`:

```json
{
  "Agent": {
    "WeatherApiKey": "your-weather-api-key",
    "SearchApiKey": "your-search-api-key",
    "SearchEngineId": "your-search-engine-id"
  }
}
```

## Adding Custom Tools

Extend the agent with your own custom tools:

```csharp
// Create a custom tool
var customTool = new AgentTool
{
    Name = "MyCustomTool",
    Description = "Does something useful",
    Parameters = new Dictionary<string, ToolParameter>
    {
        ["param1"] = new ToolParameter
        {
            Name = "param1",
            Description = "A parameter",
            Type = "string",
            Required = true
        }
    },
    Execute = async (parameters) =>
    {
        // Tool implementation
        var param1 = parameters["param1"].ToString();
        return new { Result = $"Processed {param1}" };
    }
};

// Register the tool
agentService.RegisterTool(customTool);
```

## Best Practices

1. **Use Context IDs**: Always use context IDs for related requests to maintain conversation flow
2. **Provide Clear Instructions**: Be specific about what you want the agent to do
3. **Start Simple**: Begin with simple tasks before moving to complex ones
4. **Handle Failures**: Check response status and handle failures gracefully
5. **Secure API Keys**: Store API keys securely and don't expose them in client code

## Future Enhancements

1. **Tool Chaining**: Define sequences of tools for common workflows
2. **Memory Management**: Improve long-term memory for agents
3. **User Preferences**: Personalize agent behavior based on user preferences
4. **Advanced Reasoning**: Enhance the agent's problem-solving capabilities
5. **Visual Tools**: Add tools that can process and generate images
