using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ThirdThursdayBot.Models
{
    public class YelpAuthenticationResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}