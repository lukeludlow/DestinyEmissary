using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmissaryCore
{
    public class OAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        // TODO process expire time for token, and get refresh token to refresh when needed.
        // but for now i'm lazy and i will just let access expire every 60 mins.

        // this will only exist if the request fails and a 400 response is sent with the error description in the body
        [JsonProperty("error_description")]
        public string ErrorMessage { get; set; }
    }
}