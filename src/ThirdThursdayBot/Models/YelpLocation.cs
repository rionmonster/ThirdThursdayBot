using Newtonsoft.Json;

namespace ThirdThursdayBot.Models
{
    public class YelpLocation
    {
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("address1")]
        public string Address { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }

        public string FullAddress => $"{Address}, {City}, {State} {ZipCode}";
    }
}