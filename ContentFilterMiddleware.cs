using System;
using System.Collections.Generic;
using System.Linq;

namespace AIAgentGuardrails
{
    public class ContentFilterMiddleware : IAgentMiddleware
    {
        private readonly List<string> _bannedKeywords;

        public ContentFilterMiddleware(List<string> bannedKeywords)
        {
            _bannedKeywords = bannedKeywords.Select(kw => kw.ToLower()).ToList();
        }

        public MiddlewareResult BeforeAgent(AgentState state, Runtime runtime)
        {
            if (state.Messages == null || !state.Messages.Any())
            {
                return null;
            }

            var firstMessage = state.Messages.First();
            if (firstMessage.Type != MessageType.Human)
            {
                return null;
            }

            string content = firstMessage.Content.ToLower();
            foreach (var keyword in _bannedKeywords)
            {
                if (content.Contains(keyword))
                {
                    return new MiddlewareResult
                    {
                        Messages = new List<Message>
                        {
                            new Message("assistant", "I cannot process inappropriate requests.", MessageType.AIMessage)
                        },
                        JumpTo = "end"
                    };
                }
            }
            return null;
        }

        public MiddlewareResult AfterAgent(AgentState state, Runtime runtime)
        {
            return null; // This middleware only acts before the agent
        }
    }
}
