using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace Emissary
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class UserMembershipsResponse
    {
        [JsonProperty("Response.destinyMemberships")]
        public DestinyMembership[] DestinyMemberships { get; set; }
    }
}