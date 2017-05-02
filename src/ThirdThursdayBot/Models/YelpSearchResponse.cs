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