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

                if (Regex.IsMatch(message, "show|all", RegexOptions.IgnoreCase))
                {
                    await ReplyWithRestaurantListingAsync(activity, connector);
                }
                else if (Regex.IsMatch(message, "(?<=have we been to )(?<restaurant>[^?]+)", RegexOptions.IgnoreCase))
                {
                    var restaurant = Regex.Match(message, @"(?<=have we been to )(?<restaurant>[^?]+)", RegexOptions.IgnoreCase)?.Groups["restaurant"]?.Value ?? "";
                    if (!string.IsNullOrWhiteSpace(restaurant))
                    {
                        // TODO: Make this more efficient
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

                    await ReplyWithUnrecognizableRestaurantAsync(activity, connector);
                }
                else if (Regex.IsMatch(message, "who's turn|who is next", RegexOptions.IgnoreCase))
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

                var nextMember = members[members.Length - 1];
                var nextMonth = lastRestaurantVisited?.Date.AddMonths(1) ?? DateTime.Now.AddMonths(1);

                var replyMessage = string.Format(Constants.NextChooserFormattingMessage, nextMember, nextMonth.ToString("MMM"));
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
            var reply = activity.CreateReply(Constants.DefaultResponseMessage);

            return await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<ResourceResponse> ReplyWithVisitedRestaurantAsync(Restaurant restaurant, Activity activity, ConnectorClient connector)
        {
            var replyMessage = string.Format(Constants.PreviouslyChosenResturantFormattingMessage, restaurant.Location, restaurant.PickedBy, restaurant.Date);
            var reply = activity.CreateReply(replyMessage);

            return await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<ResourceResponse> ReplyWithUnchosenRestaurantAsync(string restaurant, Activity activity, ConnectorClient connector)
        {
            var replyMessage = string.Format(Constants.UnchosenRestaurantFormattingMessage, restaurant);
            var reply = activity.CreateReply(replyMessage);

            return await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<ResourceResponse> ReplyWithUnrecognizableRestaurantAsync(Activity activity, ConnectorClient connector)
        {
            var reply = activity.CreateReply(Constants.UnrecognizableRestaurantMessage);

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

                var message = new StringBuilder(Constants.RestaurantListingMessage);
                foreach (var restaurant in restaurants)
                {
                    message.AppendLine($"- '{restaurant.Location}' on {restaurant.Date} ({restaurant.PickedBy})");
                }

                return message.ToString();
            }
            catch
            {
                return Constants.DatabaseAccessIssuesMessage;
            }
        }

        private async Task<string[]> GetAllMembers()
        {
            var json = await _client.GetStringAsync("/Members/.json");

            return JsonConvert.DeserializeObject<string[]>(json);
        }
    }
}