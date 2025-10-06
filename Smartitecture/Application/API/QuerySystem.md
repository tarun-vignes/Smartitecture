# Smartitecture Query System Documentation

## Overview

The Smartitecture Query System provides a structured approach to interacting with the AI assistant. Unlike simple chat messages, queries offer a more powerful way to request information and actions with specific parameters, context management, and structured responses.

## Key Components

### 1. Query Models

- **QueryDto**: Represents a structured query with:
  - Type (command, question, search)
  - Action (specific intent)
  - Parameters (key-value pairs)
  - Context ID (for conversation continuity)

- **QueryResponseDto**: Represents a structured response with:
  - Status (success, failed, partial)
  - Textual response
  - Structured data
  - Actions performed
  - Suggested follow-up queries

### 2. Query Service

The `QueryService` provides the core functionality:

- **Process queries**: Handles different query types (commands, questions, searches)
- **Parse text to queries**: Converts natural language to structured queries
- **Context management**: Maintains conversation history for follow-up queries
- **Query suggestions**: Generates contextual follow-up suggestions

### 3. API Endpoints

The `QueryController` exposes these endpoints:

- **POST /api/query**: Process a structured query
- **POST /api/query/parse**: Parse text into a structured query
- **GET /api/query/suggestions**: Get suggested follow-up queries

## Query Types

### Command Queries

Command queries execute system actions:

```json
{
  "type": "command",
  "action": "LaunchApp",
  "text": "Open Notepad",
  "parameters": {
    "param1": "notepad"
  }
}
```

### Question Queries

Question queries get information from the AI:

```json
{
  "type": "question",
  "action": "answer",
  "text": "What is the capital of France?",
  "parameters": {}
}
```

### Search Queries

Search queries look for specific information:

```json
{
  "type": "search",
  "action": "search",
  "text": "Find files with .docx extension",
  "parameters": {
    "terms": "files with .docx extension"
  }
}
```

## Context Management

The query system maintains conversation context using a `contextId`:

1. Client generates a unique `contextId` for a conversation session
2. Each query in the conversation includes this ID
3. The system uses previous queries to understand context
4. Follow-up queries can reference previous information

Example flow:

```
Query 1: "What is Windows 11?" (contextId: "session123")
Query 2: "When was it released?" (contextId: "session123") 
         → System knows "it" refers to Windows 11
```

## Query Suggestions

The system provides contextual suggestions for follow-up queries:

- Based on query type (command, question, search)
- Considers conversation history
- Provides natural next steps

## Integration with Existing Components

The Query System integrates with:

- **ILLMService**: For natural language understanding
- **CommandMapper**: For executing system commands
- **Chat API**: Complements the simpler chat interface

## Usage Examples

### Processing a Query

```csharp
// Create a query
var query = new QueryDto
{
    Type = "command",
    Action = "LaunchApp",
    Text = "Open Notepad",
    Parameters = new Dictionary<string, object>
    {
        { "param1", "notepad" }
    },
    ContextId = "session123"
};

// Process the query
var response = await queryService.ProcessQueryAsync(query);
```

### Parsing Text to a Query

```csharp
// Parse natural language to a query
var query = await queryService.ParseTextToQueryAsync(
    "What's the weather like today?", 
    "session123");
```

### Getting Query Suggestions

```csharp
// Get suggestions for follow-up queries
var suggestions = await queryService.GetQuerySuggestionsAsync(
    "session123", 
    3);
```

## Best Practices

1. **Use Context IDs**: Always use context IDs for related queries to maintain conversation flow
2. **Provide Specific Types**: Specify query type when known for more accurate processing
3. **Include Parameters**: Use structured parameters for precise control
4. **Handle Failures**: Check response status and handle failures gracefully
5. **Offer Suggestions**: Present suggested queries to users for better UX

## Future Enhancements

1. **Query Templates**: Predefined templates for common query patterns
2. **Advanced Parameter Extraction**: More sophisticated parameter parsing from natural language
3. **Multi-turn Queries**: Complex queries that require multiple steps
4. **Query History UI**: Interface for browsing and reusing past queries
5. **Query Analytics**: Tracking and analysis of query patterns
