# AIGuardrails

A C# demonstration project showcasing AI agent guardrails through middleware-based architecture. This project implements various safety and compliance mechanisms for AI agents, including content filtering, PII detection and handling, safety guardrails, and human-in-the-loop processing.

## Overview

AI agents can pose risks if not properly safeguarded. This project demonstrates a middleware pattern for implementing guardrails that can:

- Filter inappropriate content
- Detect and handle Personally Identifiable Information (PII)
- Enforce safety protocols
- Enable human oversight when needed

The architecture uses a pipeline of middleware that processes agent requests before and after the core AI logic executes.

## Project Structure

- `Program.cs` - Main entry point with demonstration scenarios
- `Models.cs` - Core data models and interfaces (Message, AgentState, IAgentMiddleware, etc.)
- `ContentFilterMiddleware.cs` - Filters requests containing banned keywords
- `PIIMiddleware.cs` - Detects and handles PII (emails, credit cards, API keys)
- `SafetyGuardrailMiddleware.cs` - Blocks requests related to dangerous activities
- `HumanInTheLoopMiddleware.cs` - Pauses processing for human review when needed
- `AIGuardrails.csproj` - Project configuration
- `AIGuardrails.sln` - Solution file

## Prerequisites

- .NET 10.0 SDK (or compatible version)
- A C# development environment (Visual Studio, VS Code with C# extension, etc.)

## Building and Running

1. Clone or navigate to the project directory
2. Build the project:
   ```bash
   dotnet build
   ```
3. Run the demonstration:
   ```bash
   dotnet run
   ```

The console application will run through several demo scenarios showing each middleware in action, including:
- Content filtering for banned keywords
- PII detection and redaction/masking/blocking
- Safety guardrails for dangerous content
- Combined guardrails working together

## Usage

The project demonstrates a modular approach to AI safety:

1. **Agent Class**: Core agent that processes requests through middleware pipeline
2. **Middleware Interface**: `IAgentMiddleware` with `BeforeAgent` and `AfterAgent` hooks
3. **Strategies**: Different approaches for handling detected issues (block, redact, mask, pause)

### Example Usage

```csharp
// Create middleware
var contentFilter = new ContentFilterMiddleware(new List<string> { "hack", "exploit" });
var piiMiddleware = new PIIMiddleware("email", PIIStrategy.Redact, applyToInput: true);

// Create agent with guardrails
var agent = new Agent(new List<IAgentMiddleware> { contentFilter, piiMiddleware });

// Process a request
var state = new AgentState();
state.Messages.Add(new Message("user", "How to hack a system?", MessageType.Human));
var result = agent.Invoke(state);
```

## Extending the Project

To add new guardrails:

1. Implement `IAgentMiddleware`
2. Override `BeforeAgent` and/or `AfterAgent` methods
3. Return `MiddlewareResult` to modify state or control flow
4. Add your middleware to the agent's pipeline

## Contributing

This is a demonstration project. Feel free to extend it with additional middleware types, more sophisticated detection algorithms, or integration with real AI services.

## License

[Add appropriate license information]