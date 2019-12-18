using System;

namespace Emissary
{
    public class UserDoesNotExistException : Exception
    {
        public UserDoesNotExistException()
        {
        }
        public UserDoesNotExistException(string message)
            : base(message)
        {
        }
    }
}