using System;

namespace Emissary
{
    public class EmissaryDataAccessException : Exception
    {
        public EmissaryDataAccessException()
        {
        }
        public EmissaryDataAccessException(string message)
            : base(message)
        {
        }
    }
}