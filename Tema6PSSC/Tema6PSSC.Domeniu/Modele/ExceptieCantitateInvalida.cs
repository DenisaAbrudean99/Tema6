using System;
using System.Runtime.Serialization;

namespace Tema6PSSC.Domeniu.Modele
{
    [Serializable]
    internal class ExceptieCantitateInvalida : Exception
    {
        public ExceptieCantitateInvalida()
        {
        }

        public ExceptieCantitateInvalida(string? message) : base(message)
        {
        }

        public ExceptieCantitateInvalida(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ExceptieCantitateInvalida(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
