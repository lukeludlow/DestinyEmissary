using System;
using System.Net.Http;

namespace Emissary
{
    public class OAuthRequest
    {
        public string AuthCode { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public OAuthRequest(string authCode, string clientId, string clientSecret)
        {
            this.AuthCode = authCode;
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
        }
    }
}