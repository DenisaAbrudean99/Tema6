using System;
using System.Runtime.Serialization;

namespace Tema6PSSC.Domeniu.Modele
{
    [Serializable]
    internal class ExceptiePretInvalid : Exception
    {
        public ExceptiePretInvalid()
        {
        }

        public ExceptiePretInvalid(string? message) : base(message)
        {
        }

        public ExceptiePretInvalid(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ExceptiePretInvalid(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
