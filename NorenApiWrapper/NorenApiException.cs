using System;
using System.Runtime.Serialization;

namespace NorenRestApiWrapper
{
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UndefinedVariableException()
        {
        }

        public UndefinedVariableException(string message) : base(message)
        {
        }

        public UndefinedVariableException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UndefinedVariableException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}