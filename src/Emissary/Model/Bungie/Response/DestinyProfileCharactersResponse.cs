using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Emissary
{
    /// <summary>
    /// https://bungie-net.github.io/multi/schema_Destiny-Responses-DestinyProfileResponse.html#schema_Destiny-Responses-DestinyProfileResponse
    /// 'characters' object property
    /// Basic information about each character, keyed by the CharacterId.
    /// COMPONENT TYPE: Characters
    /// </summary>
    public class DestinyProfileCharactersResponse
    {

        [JsonProperty("Response")]
        public CharactersResponse Response { get; set; }

        [JsonProperty("ErrorCode")]
        public long ErrorCode { get; set; }

        [JsonProperty("ErrorStatus")]
        public string ErrorStatus { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }

    public class CharactersResponse
    {
        [JsonProperty("characters")]
        public CharacterData Characters { get; set; }
    }

    public class CharacterData
    {
        [JsonProperty("data")]
        public Dictionary<long, DestinyCharacterComponent> Data { get; set; }
    }

    /// <summary>
	/// https://bungie-net.github.io/multi/schema_Destiny-Entities-Characters-DestinyCharacterComponent.html#schema_Destiny-Entities-Characters-DestinyCharacterComponent
	/// </summary>
    public class DestinyCharacterComponent
    {
        [JsonProperty("membershipId")]
        public long MembershipId { get; set; }

        [JsonProperty("membershipType")]
        public long MembershipType { get; set; }

        [JsonProperty("characterId")]
        public long CharacterId { get; set; }

        [JsonProperty("dateLastPlayed")]
        public DateTimeOffset DateLastPlayed { get; set; }

        [JsonProperty("raceHash")]
        public UInt32 RaceHash { get; set; }

        [JsonProperty("genderHash")]
        public UInt32 GenderHash { get; set; }

        [JsonProperty("classHash")]
        public UInt32 ClassHash { get; set; }
    }



}