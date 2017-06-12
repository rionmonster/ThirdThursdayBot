using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using System;
using System.Text;
using System.Text.RegularExpressions;
using ThirdThursdayBot.Models;
using Newtonsoft.Json;
using System.Linq;

namespace ThirdThursdayBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private HttpClient _client;
        private static string _yelpAuthenticationToken;

        public MessagesController()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(Environment.GetEnvironmentVariable("DatabaseEndpoint"))
            };
        }

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            if (activity.Type == ActivityTypes.Message)
            {
                var message = activity.Text;

                if (Regex.IsMatch(message, "(?<=have we been to )(?<restaurant>[^?]+)", RegexOptions.IgnoreCase))
                {
                    var restaurant = Regex.Match(message, @"(?<=have we been to )(?<restaurant>[^?]+)", RegexOptions.IgnoreCase)?.Groups["restaurant"]?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(restaurant))
                    {
                        var vistedRestaurants = await GetAllVisitedRestaurantsAsync();
                        var visitedRestaurant = vistedRestaurants.FirstOrDefault(r => string.Equals(r.Location, restaurant, StringComparison.OrdinalIgnoreCase));
                        if (visitedRestaurant != null)
                        {
                            await ReplyWithVisitedRestaurantAsync(visitedRestaurant, activity, connector);
                        }
                        else
                        {
                            await ReplyWithUnchosenRestaurantAsync(restaurant, activity, connector);
                        }
                    }
                    else
                    {
                        await ReplyWithUnrecognizableRestaurantAsync(activity, connector);
                    }
                }
                else if (Regex.IsMatch(message, "where should we go|recommendation|pick for me", RegexOptions.IgnoreCase))
                {
                    await ReplyWithRandomRestaurantRecommendation(activity, connector);
                }
                else if (Regex.IsMatch(message, "show|all|list all", RegexOptions.IgnoreCase))
                {
                    await ReplyWithRestaurantListingAsync(activity, connector);
                }
                else if (Regex.IsMatch(message, "who's next|who is next|whose (pick|turn) is it", RegexOptions.IgnoreCase))
                {
                    await ReplyWithNextMemberToChoose(activity, connector);
                }
                else
                {
                    await ReplyWithDefaultMessageAsync(activity, connector);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private async Task<ResourceResponse> ReplyWithNextMemberToChoose(Activity activity, ConnectorClient connector)
        {
            try
            {
                var lastRestaurantVisited = await GetLastVisitedRestaurantAsync();
                var members = await GetAllMembers();

                var currentMember = Array.IndexOf(members, lastRestaurantVisited?.PickedBy ?? "");
                var nextMember = members[(currentMember + 1) % members.Length];
                var nextMonth = lastRestaurantVisited?.Date.AddMonths(1) ?? DateTime.Now.AddMonths(1);

                var replyMessage = string.Format(Messages.NextChooserFormattingMessage, nextMember, nextMonth.ToString("MMMM"));
                var reply = activity.CreateReply(replyMessage);
                return await connector.Conversations.ReplyToActivityAsync(reply);
            }
            catch
            {
                var reply = activity.CreateReply("I'm not sure who has the next pick. Try again later.");
                return await connector.Conversations.ReplyToActivityAsync(reply);
            }
        }

        private async Task<ResourceResponse> ReplyWithDefaultMessageAsync(Activity activity, ConnectorClient connector)
        {
            var reply = activity.CreateReply(Messages.DefaultResponseMessage);

            return await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<ResourceResponse> ReplyWithVisitedRestaurantAsync(Restaurant restaurant, Activity activity, ConnectorClient connector)
        {
            var replyMessage = string.Format(Messages.PreviouslyChosenResturantFormattingMessage, restaurant.Location, restaurant.PickedBy, restaurant.Date);
            var reply = activity.CreateReply(replyMessage);

            return await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<ResourceResponse> ReplyWithUnchosenRestaurantAsync(string restaurant, Activity activity, ConnectorClient connector)
        {
            var replyMessage = string.Format(Messages.UnchosenRestaurantFormattingMessage, restaurant);
            var reply = activity.CreateReply(replyMessage);

            return await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<ResourceResponse> ReplyWithUnrecognizableRestaurantAsync(Activity activity, ConnectorClient connector)
        {
            var reply = activity.CreateReply(Messages.UnrecognizableRestaurantMessage);

            return await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<ResourceResponse> ReplyWithRestaurantListingAsync(Activity activity, ConnectorClient connector)
        {
            var replyMessage = await GetPreviouslyVisitedRestaurantsMessageAsync();
            var reply = activity.CreateReply(replyMessage);

            return await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<Restaurant[]> GetAllVisitedRestaurantsAsync()
        {
            var json = await _client.GetStringAsync("/Restaurants/.json");

            return JsonConvert.DeserializeObject<Restaurant[]>(json);
        }

        private async Task<Restaurant> GetLastVisitedRestaurantAsync()
        {
            var restaurants = await GetAllVisitedRestaurantsAsync();
            return restaurants.LastOrDefault();
        }

        private async Task<string> GetPreviouslyVisitedRestaurantsMessageAsync()
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

        private async Task<string[]> GetAllMembers()
        {
            var json = await _client.GetStringAsync("/Members/.json");

            return JsonConvert.DeserializeObject<string[]>(json);
        }

        private async Task<ResourceResponse> ReplyWithRandomRestaurantRecommendation(Activity activity, ConnectorClient connector)
        {
            try
            {
                using (var yelpClient = new HttpClient())
                {
                    await EnsureYelpAuthentication(yelpClient);

                    if (!string.IsNullOrWhiteSpace(_yelpAuthenticationToken))
                    {
                        var response = await GetYelpSearchQuery(yelpClient);
                        var visitedRestaurants = await GetAllVisitedRestaurantsAsync();
                        var recommendation = response.Restaurants
                                                     .OrderBy(r => Guid.NewGuid())
                                                     .First(r => visitedRestaurants.All(v => !v.Location.Contains(r.Name) && !r.Name.Contains(v.Location)));

                        var recommendationMessage = activity.CreateReply(GetFormattedRecommendation(recommendation)); 
                        return await connector.Conversations.ReplyToActivityAsync(recommendationMessage);
                    }

                    var reply = activity.CreateReply(Messages.UnableToGetRecommendationMessage);
                    return await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            catch (Exception ex)
            {
                var failedMessage = activity.CreateReply(Messages.UnableToGetRecommendationMessage);
                return await connector.Conversations.ReplyToActivityAsync(failedMessage);
            }
        }
        
        private async Task EnsureYelpAuthentication(HttpClient yelpClient)
        {
            if (string.IsNullOrWhiteSpace(_yelpAuthenticationToken))
            {
                var authenticationResponse = await yelpClient.PostAsync($"https://api.yelp.com/oauth2/token?client_id={Environment.GetEnvironmentVariable("YelpClientId")}&client_secret={Environment.GetEnvironmentVariable("YelpClientSecret")}&grant_type=client_credentials", null);
                if (authenticationResponse.IsSuccessStatusCode)
                {
                    var authResponse = JsonConvert.DeserializeObject<YelpAuthenticationResponse>(await authenticationResponse.Content.ReadAsStringAsync());
                    _yelpAuthenticationToken = authResponse.AccessToken;
                }
            }
        }

        private async Task<YelpSearchResponse> GetYelpSearchQuery(HttpClient yelpClient)
        {
            yelpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_yelpAuthenticationToken}");
            var searchTerms = new[]
            {
                            $"term=food",
                            $"location={Environment.GetEnvironmentVariable("YelpPreferredLocation")}",
                            $"limit=50"
            };

            var searchRequest = await yelpClient.GetStringAsync($"https://api.yelp.com/v3/businesses/search?{string.Join("&", searchTerms)}");
            return JsonConvert.DeserializeObject<YelpSearchResponse>(searchRequest);
        }

        private string GetFormattedRecommendation(YelpBusiness choice)
        {
            return string.Format(Messages.RecommendationFormattingMessage, 
                choice.Name, 
                choice.Rating, 
                choice.Location.FullAddress, 
                choice.PhoneNumber);
        }
    }
}