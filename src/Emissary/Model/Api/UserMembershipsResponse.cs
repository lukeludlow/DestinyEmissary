using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class UserMembershipsResponse
    {
        [JsonProperty("Response.destinyMemberships")]
        public DestinyMembership[] DestinyMemberships { get; set; }
    }
}