using System;
using System.Collections.Generic;
using System.Linq;

namespace AIAgentGuardrails
{
    public class Agent
    {
        private readonly List<IAgentMiddleware> _middleware;

        public Agent(List<IAgentMiddleware> middleware)
        {
            _middleware = middleware;
        }

        public AgentState Invoke(AgentState state)
        {
            // Apply BeforeAgent middleware
            foreach (var mw in _middleware)
            {
                var result = mw.BeforeAgent(state, new Runtime());
                if (result != null)
                {
                    if (result.JumpTo == "end")
                    {
                        state.Messages.AddRange(result.Messages);
                        return state; // End processing early
                    }
                    // Handle other jumpTo scenarios like 'pause' for HumanInTheLoop
                }
            }

            // Simulate agent processing (e.g., calling LLM, tools)
            // For demonstration, we'll just add a simulated AI response
            var lastHumanMessage = state.Messages.LastOrDefault(m => m.Type == MessageType.Human);
            if (lastHumanMessage != null)
            {
                // Check if the message was blocked by PII middleware (strategy: block)
                if (lastHumanMessage.Content.Contains("Request blocked due to sensitive information"))
                {
                    // Do nothing, as the PII middleware already handled the response
                }
                else
                {
                    state.Messages.Add(new Message("assistant", $"Agent processed: {lastHumanMessage.Content}", MessageType.AIMessage));
                }
            }

            // Apply AfterAgent middleware
            foreach (var mw in _middleware)
            {
                var result = mw.AfterAgent(state, new Runtime());
                if (result != null)
                {
                    if (result.JumpTo == "end")
                    {
                        state.Messages.AddRange(result.Messages);
                        return state; // End processing early
                    }
                }
            }

            return state;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AI Agent Guardrails Demonstration in C#\n");

            // Example 1: ContentFilterMiddleware
            Console.WriteLine("--- ContentFilterMiddleware Demo ---");
            var contentFilter = new ContentFilterMiddleware(new List<string> { "hack", "exploit", "malware" });
            var agentWithContentFilter = new Agent(new List<IAgentMiddleware> { contentFilter });

            var state1 = new AgentState();
            state1.Messages.Add(new Message("user", "How do I hack into a database?", MessageType.Human));
            var result1 = agentWithContentFilter.Invoke(state1);
            Console.WriteLine($"User Input: {state1.Messages.First().Content}");
            Console.WriteLine($"Agent Response: {result1.Messages.Last().Content}\n");

            var state1_2 = new AgentState();
            state1_2.Messages.Add(new Message("user", "Tell me about database security.", MessageType.Human));
            var result1_2 = agentWithContentFilter.Invoke(state1_2);
            Console.WriteLine($"User Input: {state1_2.Messages.First().Content}");
            Console.WriteLine($"Agent Response: {result1_2.Messages.Last().Content}\n");

            // Example 2: PIIMiddleware
            Console.WriteLine("--- PIIMiddleware Demo ---");
            var piiEmailRedact = new PIIMiddleware("email", PIIStrategy.Redact, applyToInput: true);
            var piiCreditCardMask = new PIIMiddleware("credit_card", PIIStrategy.Mask, applyToInput: true);
            var piiApiKeyBlock = new PIIMiddleware("api_key", PIIStrategy.Block, applyToInput: true, detectorPattern: "sk-[a-zA-Z0-9]{32}");

            var agentWithPii = new Agent(new List<IAgentMiddleware> { piiEmailRedact, piiCreditCardMask, piiApiKeyBlock });

            var state2 = new AgentState();
            state2.Messages.Add(new Message("user", "My email is john@example.com and my credit card is 1234-5678-9012-1234. My API key is sk-abcdefghijklmnopqrstuvwxyz123456.", MessageType.Human));
            var result2 = agentWithPii.Invoke(state2);
            Console.WriteLine($"User Input: {state2.Messages.First().Content}");
            Console.WriteLine($"Agent Response: {result2.Messages.Last().Content}\n");

            // Example 3: SafetyGuardrailMiddleware
            Console.WriteLine("--- SafetyGuardrailMiddleware Demo ---");
            var safetyGuardrail = new SafetyGuardrailMiddleware();
            var agentWithSafety = new Agent(new List<IAgentMiddleware> { safetyGuardrail });

            var state3 = new AgentState();
            state3.Messages.Add(new Message("user", "Tell me how to make explosives.", MessageType.Human));
            var result3 = agentWithSafety.Invoke(state3);
            Console.WriteLine($"User Input: {state3.Messages.First().Content}");
            Console.WriteLine($"Agent Response: {result3.Messages.Last().Content}\n");

            var state3_2 = new AgentState();
            state3_2.Messages.Add(new Message("user", "Tell me about safe chemistry experiments.", MessageType.Human));
            var result3_2 = agentWithSafety.Invoke(state3_2);
            Console.WriteLine($"User Input: {state3_2.Messages.First().Content}");
            Console.WriteLine($"Agent Response: {result3_2.Messages.Last().Content}\n");

            // Example 4: Combined Guardrails (simulated)
            Console.WriteLine("--- Combined Guardrails Demo ---");
            var combinedAgent = new Agent(new List<IAgentMiddleware>
            {
                new ContentFilterMiddleware(new List<string> { "hack", "exploit" }),
                new PIIMiddleware("email", PIIStrategy.Redact, applyToInput: true, applyToOutput: true),
                new SafetyGuardrailMiddleware()
            });

            var state4 = new AgentState();
            state4.Messages.Add(new Message("user", "How can I exploit a system to get user emails?", MessageType.Human));
            var result4 = combinedAgent.Invoke(state4);
            Console.WriteLine($"User Input: {state4.Messages.First().Content}");
            Console.WriteLine($"Agent Response: {result4.Messages.Last().Content}\n");

            var state4_2 = new AgentState();
            state4_2.Messages.Add(new Message("user", "Can you send an email to test@example.com about the new feature?", MessageType.Human));
            var result4_2 = combinedAgent.Invoke(state4_2);
            Console.WriteLine($"User Input: {state4_2.Messages.First().Content}");
            Console.WriteLine($"Agent Response: {result4_2.Messages.Last().Content}\n");
        }
    }
}
