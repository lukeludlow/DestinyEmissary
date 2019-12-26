using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class UserMembershipsResponse
    {
        [JsonProperty("Response.destinyMemberships")]
        public IList<DestinyMembership> DestinyMemberships { get; set; }
    }
}