using System;
using System.Collections.Generic;
using System.Linq;

namespace AIAgentGuardrails
{
    public class HumanInTheLoopMiddleware : IAgentMiddleware
    {
        private readonly Dictionary<string, bool> _interruptOn;
        private readonly Dictionary<string, AgentState> _pausedStates = new Dictionary<string, AgentState>();

        public HumanInTheLoopMiddleware(Dictionary<string, bool> interruptOn)
        {
            _interruptOn = interruptOn;
        }

        public MiddlewareResult BeforeAgent(AgentState state, Runtime runtime)
        {
            // This middleware primarily acts on tool calls, which would typically be handled within the agent's logic
            // or by a custom tool execution mechanism. For this C# conversion, we'll simulate the interruption
            // based on a hypothetical 'next action' in the state or by explicitly checking for a 'Command' to resume.

            // If a command to resume is present, it means a human decision has been made.
            // This part would need integration with how the agent's 'invoke' method handles commands.
            // For now, we'll assume the 'resume' logic is handled externally or in a higher-level agent runner.

            return null;
        }

        public MiddlewareResult AfterAgent(AgentState state, Runtime runtime)
        {
            // This is where we would check if the agent decided to call a tool that requires human approval.
            // In a real C# agent framework, the tool call would be intercepted before execution.
            // For this example, we'll assume the 'state' might contain an indication of a pending risky action.
            // This part is more conceptual without a full agent framework.

            // Example: If the agent's last message indicates a tool call that needs approval
            // This is a simplification; actual implementation would involve parsing agent's planned actions.
            var lastMessage = state.Messages.LastOrDefault();
            if (lastMessage != null && lastMessage.Type == MessageType.AIMessage && lastMessage.Content.StartsWith("CALL_TOOL:"))
            {
                string toolName = lastMessage.Content.Split(':')[1].Trim();
                if (_interruptOn.ContainsKey(toolName) && _interruptOn[toolName])
                {
                    // Simulate pausing the agent and returning control for human approval
                    // In a real system, this would involve saving the state and notifying the user interface.
                    // For this conversion, we'll return a result that indicates a pause.
                    return new MiddlewareResult
                    {
                        Messages = new List<Message>
                        {
                            new Message("assistant", $"Agent requires human approval for tool: {toolName}", MessageType.AIMessage)
                        },
                        JumpTo = "pause"
                    };
                }
            }
            return null;
        }

        // This method would be called externally to resume the agent after human approval
        public void ResumeAgent(string threadId, bool approved)
        {
            if (_pausedStates.ContainsKey(threadId))
            {
                AgentState pausedState = _pausedStates[threadId];
                // Modify the pausedState based on approval and re-inject into agent processing
                // This is a placeholder for actual resume logic.
                _pausedStates.Remove(threadId);
            }
        }
    }
}
