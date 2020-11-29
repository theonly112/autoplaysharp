using System;
using System.Runtime.Serialization;

namespace autoplaysharp.Emulators
{
    [Serializable]
    public class FailedToFindWindowException : Exception
    {
        public FailedToFindWindowException()
        {
        }

        public FailedToFindWindowException(string message) : base(message)
        {
        }

        public FailedToFindWindowException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FailedToFindWindowException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
