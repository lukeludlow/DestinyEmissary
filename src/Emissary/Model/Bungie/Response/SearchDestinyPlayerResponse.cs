using Newtonsoft.Json;

namespace Emissary
{
    /// <summary>
    /// https://bungie-net.github.io/multi/operation_get_Destiny2-SearchDestinyPlayer.html#operation_get_Destiny2-SearchDestinyPlayer
    /// </summary>
    public class SearchDestinyPlayerResponse
    {

        [JsonProperty("Response")]
        public UserInfoCard[] Response { get; set; }

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
}