using System.Collections.Generic;
using VSItemTooltips.Core.Abstractions;

namespace VSEvolutionHelper.Tests.Mocks
{
    public class MockLogger : IModLogger
    {
        public List<string> Messages { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        public List<string> Errors { get; } = new List<string>();

        public void Msg(string message) => Messages.Add(message);
        public void Warning(string message) => Warnings.Add(message);
        public void Error(string message) => Errors.Add(message);

        public void Clear()
        {
            Messages.Clear();
            Warnings.Clear();
            Errors.Clear();
        }
    }
}
