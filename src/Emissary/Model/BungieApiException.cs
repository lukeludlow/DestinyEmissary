using System;

namespace EmissaryCore
{
    public class BungieApiException : Exception
    {
        public BungieApiException(string message)
            : base(message)
        {
        } 
    }
}