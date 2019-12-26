using System;

namespace EmissaryCore
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