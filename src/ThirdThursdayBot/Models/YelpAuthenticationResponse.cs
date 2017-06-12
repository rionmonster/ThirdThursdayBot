using Newtonsoft.Json;

namespace ThirdThursdayBot.Models
{
    public class YelpAuthenticationResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}