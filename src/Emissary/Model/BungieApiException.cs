using System;

namespace Emissary
{
    public class BungieApiException : Exception
    {
        public BungieApiException(string message)
            : base(message)
        {
        } 
    }
}