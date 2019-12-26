using System;
using System.Net.Http;

namespace EmissaryCore
{
    public class UserMembershipsRequest
    {
        public string AccessToken { get; set; }

        public UserMembershipsRequest(string accessToken)
        {
            this.AccessToken = accessToken;
        }
    }
}