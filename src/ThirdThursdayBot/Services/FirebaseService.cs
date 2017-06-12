using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ThirdThursdayBot.Models;

namespace ThirdThursdayBot.Services
{
    public class FirebaseService : IFirebaseService
    {
        private HttpClient _client;

        public FirebaseService(string firebaseEndpoint)
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(firebaseEndpoint)
            };
        }

        public async Task<Restaurant[]> GetAllVisitedRestaurantsAsync()
        {
            var json = await _client.GetStringAsync("/Restaurants/.json");

            return JsonConvert.DeserializeObject<Restaurant[]>(json);
        }

        public async Task<Restaurant> GetLastVisitedRestaurantAsync()
        {
            var restaurants = await GetAllVisitedRestaurantsAsync();
            return restaurants.LastOrDefault();
        }

        public async Task<string> GetPreviouslyVisitedRestaurantsMessageAsync()
        {
            try
            {
                var restaurants = await GetAllVisitedRestaurantsAsync();

                var message = new StringBuilder(Messages.RestaurantListingMessage);
                foreach (var restaurant in restaurants)
                {
                    message.AppendLine($"- '{restaurant.Location}' on {restaurant.Date.ToString("M/d/yyyy")} ({restaurant.PickedBy})");
                }

                return message.ToString();
            }
            catch
            {
                return Messages.DatabaseAccessIssuesMessage;
            }
        }

        public async Task<string[]> GetAllMembersAsync()
        {
            var json = await _client.GetStringAsync("/Members/.json");

            return JsonConvert.DeserializeObject<string[]>(json);
        }

    }
}