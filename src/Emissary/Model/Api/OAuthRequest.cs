using System;
using System.Net.Http;

namespace EmissaryCore
{
    public class OAuthRequest
    {
        public string AuthCode { get; set; }

        public OAuthRequest(string authCode)
        {
            this.AuthCode = authCode;
        }
    }
}