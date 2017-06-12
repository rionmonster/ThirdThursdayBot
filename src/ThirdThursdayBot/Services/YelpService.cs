using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ThirdThursdayBot.Models;

namespace ThirdThursdayBot.Services
{
    public class YelpService : IYelpService
    {
        private const string YelpSearchUrl = "https://api.yelp.com/v3/businesses/search?";

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _preferredLocation;
        private string _authToken;

        public YelpService(string clientId, string clientSecret, string preferredLocation = "Lake Charles")
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _preferredLocation = preferredLocation;
        }

        /// <summary>
        /// Gets a random, unvisited Restauraunt from Yelp's API
        /// </summary>
        public async Task<YelpBusiness> GetRandomUnvisitedRestaurantAsync(Restaurant[] restaurantsToExclude)
        {
            try
            {
                using (var yelpClient = new HttpClient())
                {
                    await EnsureYelpAuthenticationAsync(yelpClient);

                    if (string.IsNullOrWhiteSpace(_authToken))
                    {
                        // Yelp failed to authenticate properly, you should probably check the Client ID and Secret to ensure they are correct
                        // or you could throw an exception and log it here (i.e. YelpAuthenticationException, etc.)
                        return null;
                    }

                    var response = await GetYelpSearchQueryAsync(yelpClient);
                    var recommendation = response.Restaurants
                                                 .OrderBy(r => Guid.NewGuid())
                                                 .First(r => restaurantsToExclude.All(v => !v.Location.Contains(r.Name) && !r.Name.Contains(v.Location)));

                    return recommendation;
                }
            }
            catch
            {
                // Something else bad happened when communicating with Yelp; If you like logging, you should probably do that here
                return null;
            }
        }

        /// <summary>
        /// Ensures that the Yelp API has been authenticated for the current request
        /// </summary>
        private async Task EnsureYelpAuthenticationAsync(HttpClient yelpClient)
        {
            if (string.IsNullOrWhiteSpace(_authToken))
            {
                var authenticationResponse = await yelpClient.PostAsync($"https://api.yelp.com/oauth2/token?client_id={_clientId}&client_secret={_clientSecret}&grant_type=client_credentials", null);
                if (authenticationResponse.IsSuccessStatusCode)
                {
                    var authResponse = JsonConvert.DeserializeObject<YelpAuthenticationResponse>(await authenticationResponse.Content.ReadAsStringAsync());
                    _authToken = authResponse.AccessToken;
                }
            }
        }

        /// <summary>
        /// Sets the headers and search terms for the Yelp search query
        /// </summary>
        private async Task<YelpSearchResponse> GetYelpSearchQueryAsync(HttpClient yelpClient)
        {
            yelpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_authToken}");
            var searchTerms = new[]
            {
                $"term=food",
                $"location={_preferredLocation}",
                $"limit=50"
            };

            var searchRequest = await yelpClient.GetStringAsync($"{YelpSearchUrl}{string.Join("&", searchTerms)}");
            return JsonConvert.DeserializeObject<YelpSearchResponse>(searchRequest);
        }
    }
}