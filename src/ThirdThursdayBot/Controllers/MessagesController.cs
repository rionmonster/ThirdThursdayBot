using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using System;
using System.Text.RegularExpressions;
using ThirdThursdayBot.Models;
using System.Linq;
using ThirdThursdayBot.Services;

namespace ThirdThursdayBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly IFirebaseService _service;
        private readonly IYelpService _yelpService;

        public MessagesController()
        {
            _service = new FirebaseService(Environment.GetEnvironmentVariable("DatabaseEndpoint"));
            _yelpService = new YelpService(
                Environment.GetEnvironmentVariable("YelpClientId"),
                Environment.GetEnvironmentVariable("YelpClientSecret"),
                Environment.GetEnvironmentVariable("YelpPreferredLocation")
            );
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
                        var vistedRestaurants = await _service.GetAllVisitedRestaurantsAsync();
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
                var lastRestaurantVisited = await _service.GetLastVisitedRestaurantAsync();
                var members = await _service.GetAllMembersAsync();

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
            var replyMessage = await _service.GetPreviouslyVisitedRestaurantsMessageAsync();
            var reply = activity.CreateReply(replyMessage);

            return await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task<ResourceResponse> ReplyWithRandomRestaurantRecommendation(Activity activity, ConnectorClient connector)
        {
            try
            {
                var previouslyVisistedRestaurants = await _service.GetAllVisitedRestaurantsAsync();
                var recommendation = await _yelpService.GetRandomUnvisitedRestaurantAsync(previouslyVisistedRestaurants);

                var recommendationMessage = activity.CreateReply(GetFormattedRecommendation(recommendation)); 
                return await connector.Conversations.ReplyToActivityAsync(recommendationMessage);
            }
            catch
            {
                var failedMessage = activity.CreateReply(Messages.UnableToGetRecommendationMessage);
                return await connector.Conversations.ReplyToActivityAsync(failedMessage);
            }
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