namespace AIAgentGuardrails
{
    public enum MessageType
    {
        Human,
        AIMessage,
        ToolOutput
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public MessageType Type { get; set; }

        public Message(string role, string content, MessageType type)
        {
            Role = role;
            Content = content;
            Type = type;
        }
    }

    public class AgentState
    {
        public List<Message> Messages { get; set; } = new List<Message>();
        // Add other state properties as needed, e.g., tool outputs, intermediate thoughts
    }

    public class Runtime
    {
        // Placeholder for runtime environment information
        // e.g., configuration, available tools, etc.
    }

    public class MiddlewareResult
    {
        public List<Message> Messages { get; set; }
        public string JumpTo { get; set; }
    }

    public interface IAgentMiddleware
    {
        MiddlewareResult BeforeAgent(AgentState state, Runtime runtime);
        MiddlewareResult AfterAgent(AgentState state, Runtime runtime);
    }
}
