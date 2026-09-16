using System;

namespace ObjectSemantics.NET
{
    public class TemplateLimitExceededException : InvalidOperationException
    {
        public TemplateLimitExceededException(string message) : base(message)
        {
        }
    }
}
