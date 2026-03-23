using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIAgentGuardrails
{
    public enum PIIStrategy
    {
        Redact,
        Mask,
        Block,
        Hash
    }

    public class PIIMiddleware : IAgentMiddleware
    {
        private readonly string _piiType;
        private readonly PIIStrategy _strategy;
        private readonly Regex _detector;
        private readonly bool _applyToInput;
        private readonly bool _applyToOutput;

        public PIIMiddleware(string piiType, PIIStrategy strategy, bool applyToInput = false, bool applyToOutput = false, string detectorPattern = null)
        {
            _piiType = piiType;
            _strategy = strategy;
            _applyToInput = applyToInput;
            _applyToOutput = applyToOutput;

            if (!string.IsNullOrEmpty(detectorPattern))
            {
                _detector = new Regex(detectorPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            else
            {
                _detector = GetBuiltInDetector(piiType);
            }
        }

        private Regex GetBuiltInDetector(string piiType)
        {
            switch (piiType.ToLower())
            {
                case "email":
                    return new Regex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                case "credit_card":
                    // Simplified regex for demonstration
                    return new Regex(@"\b(?:\d{4}[-\s]?){3}\d{4}\b", RegexOptions.Compiled);
                case "api_key":
                    return new Regex(@"sk-[a-zA-Z0-9]{32}", RegexOptions.Compiled);
                default:
                    return null;
            }
        }

        private string ApplyStrategy(string text, out bool blocked)
        {
            blocked = false;
            if (_detector == null || string.IsNullOrEmpty(text)) return text;

            if (_strategy == PIIStrategy.Block && _detector.IsMatch(text))
            {
                blocked = true;
                return text;
            }

            return _detector.Replace(text, match =>
            {
                switch (_strategy)
                {
                    case PIIStrategy.Redact:
                        return $"[REDACTED_{_piiType.ToUpper()}]";
                    case PIIStrategy.Mask:
                        if (_piiType.ToLower() == "credit_card")
                        {
                            string val = match.Value.Replace("-", "").Replace(" ", "");
                            if (val.Length >= 4)
                            {
                                return "****-****-****-" + val.Substring(val.Length - 4);
                            }
                        }
                        return new string('*', match.Length);
                    case PIIStrategy.Hash:
                        using (var sha256 = System.Security.Cryptography.SHA256.Create())
                        {
                            var bytes = System.Text.Encoding.UTF8.GetBytes(match.Value);
                            var hash = sha256.ComputeHash(bytes);
                            return BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, 8) + "...";
                        }
                    default:
                        return match.Value;
                }
            });
        }

        public MiddlewareResult BeforeAgent(AgentState state, Runtime runtime)
        {
            if (!_applyToInput || state.Messages == null || !state.Messages.Any())
            {
                return null;
            }

            var firstMessage = state.Messages.First();
            if (firstMessage.Type != MessageType.Human)
            {
                return null;
            }

            bool blocked;
            string newContent = ApplyStrategy(firstMessage.Content, out blocked);

            if (blocked)
            {
                return new MiddlewareResult
                {
                    Messages = new List<Message>
                    {
                        new Message("assistant", $"Request blocked due to sensitive information ({_piiType}).", MessageType.AIMessage)
                    },
                    JumpTo = "end"
                };
            }

            firstMessage.Content = newContent;
            return null;
        }

        public MiddlewareResult AfterAgent(AgentState state, Runtime runtime)
        {
            if (!_applyToOutput || state.Messages == null || !state.Messages.Any())
            {
                return null;
            }

            var lastMessage = state.Messages.Last();
            if (lastMessage.Type != MessageType.AIMessage)
            {
                return null;
            }

            bool blocked;
            string newContent = ApplyStrategy(lastMessage.Content, out blocked);

            if (blocked)
            {
                lastMessage.Content = "I cannot provide that response due to sensitive information.";
                return null;
            }

            lastMessage.Content = newContent;
            return null;
        }
    }
}
