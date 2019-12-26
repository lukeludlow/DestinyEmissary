using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace EmissaryCore
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class ProfileCharactersResponse
    {
        [JsonProperty("Response.characters.data")]
        public Dictionary<long, DestinyCharacter> Characters { get; set; }

        public ProfileCharactersResponse(Dictionary<long, DestinyCharacter> characters)
        {
            this.Characters = characters;
        }

        public ProfileCharactersResponse()
        {
            this.Characters = new Dictionary<long, DestinyCharacter>();
        }
    }
}