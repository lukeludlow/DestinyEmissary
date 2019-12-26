using System;

namespace EmissaryCore
{
    public class DataAccessException : Exception
    {
        public DataAccessException()
        {
        }
        public DataAccessException(string message)
            : base(message)
        {
        }
    }
}