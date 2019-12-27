using System;

namespace EmissaryCore
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException()
        {
        }
        public UserNotFoundException(string message)
            : base(message)
        {
        }
    }
}