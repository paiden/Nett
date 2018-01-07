using System;

namespace Nett.Exceptions
{

    [Serializable]
    public class CircularReferenceException : Exception
    {
        public CircularReferenceException() { }
        public CircularReferenceException(string message)
            : base(message) { }
        public CircularReferenceException(string message, Exception inner)
            : base(message, inner) { }
        protected CircularReferenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
