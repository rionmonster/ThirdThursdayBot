using Newtonsoft.Json;

namespace ThirdThursdayBot.Models
{
    public class YelpBusiness
    {
        [JsonProperty("rating")]
        public double Rating { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("image_url")]
        public string Image { get; set; }
        [JsonProperty("phone")]
        public string PhoneNumber { get; set; }
        [JsonProperty("location")]
        public YelpLocation Location { get; set; }
    }
}