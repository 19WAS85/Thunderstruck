using System;

namespace Thunderstruck
{
    public class ThunderException : Exception
    {
        public ThunderException(string message) : base(message) { }

        public ThunderException(string message, Exception inner) : base(message, inner) { }
    }
}