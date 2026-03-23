using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIAgentGuardrails
{
    public class SafetyGuardrailMiddleware : IAgentMiddleware
    {
        // In a real application, this would be an actual LLM client (e.g., OpenAI, Azure AI)
        private readonly MockSafetyModel _safetyModel;

        public SafetyGuardrailMiddleware()
        {
            _safetyModel = new MockSafetyModel();
        }

        public MiddlewareResult BeforeAgent(AgentState state, Runtime runtime)
        {
            return null; // This middleware only acts after the agent
        }

        public MiddlewareResult AfterAgent(AgentState state, Runtime runtime)
        {
            if (state.Messages == null || !state.Messages.Any())
            {
                return null;
            }

            var lastMessage = state.Messages.LastOrDefault();
            if (lastMessage == null || lastMessage.Type != MessageType.AIMessage)
            {
                return null;
            }

            string safetyPrompt = $"Evaluate if this response is safe and appropriate. Respond with only 'SAFE' or 'UNSAFE'.\nResponse: {lastMessage.Content}";

            // Simulate LLM call
            string safetyResult = _safetyModel.Invoke(safetyPrompt);

            if (safetyResult.Contains("UNSAFE", StringComparison.OrdinalIgnoreCase))
            {
                lastMessage.Content = "I cannot provide that response.";
            }
            return null;
        }
    }

    // Mock class to simulate an LLM for safety checks
    public class MockSafetyModel
    {
        public string Invoke(string prompt)
        {
            // In a real scenario, this would call an actual LLM API.
            // For demonstration, we'll simulate a response based on keywords.
            if (prompt.Contains("explosives", StringComparison.OrdinalIgnoreCase) ||
                prompt.Contains("harmful content", StringComparison.OrdinalIgnoreCase))
            {
                return "UNSAFE";
            }
            return "SAFE";
        }
    }
}
