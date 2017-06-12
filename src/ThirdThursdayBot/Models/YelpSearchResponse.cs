using Newtonsoft.Json;

namespace ThirdThursdayBot.Models
{
    public class YelpSearchResponse
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("businesses")]
        public YelpBusiness[] Restaurants { get; set; }
    }
}