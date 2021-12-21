using System;
using System.Runtime.Serialization;

namespace Tema6PSSC.Domeniu.Modele
{
    [Serializable]
    internal class ExceptieCodProdusInvalid : Exception
    {
        public ExceptieCodProdusInvalid()
        {
        }

        public ExceptieCodProdusInvalid(string? message) : base(message)
        {
        }

        public ExceptieCodProdusInvalid(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ExceptieCodProdusInvalid(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
