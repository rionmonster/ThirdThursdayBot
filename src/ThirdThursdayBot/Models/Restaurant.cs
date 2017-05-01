using Newtonsoft.Json;
using System;

namespace ThirdThursdayBot.Models
{
    public class Restaurant
    {

        [JsonProperty("Location")]
        public string Location { get; private set; }

        [JsonProperty("PickedBy")]
        public string PickedBy { get; private set; }

        [JsonProperty("Date")]
        public string Date { get; private set; }

    }
}