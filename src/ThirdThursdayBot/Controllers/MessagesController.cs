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

namespace ThirdThursdayBot
{
    /// <summary>
    /// NOTE: This is super dirty code that will need to be cleaned up
    /// </summary>

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

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            if (activity.Type == ActivityTypes.Message)
            {
                // See if our response matched any of our options, if not display messages again
                var message = activity.Text.Trim().ToLower();

                if (Regex.IsMatch(message, "show|all", RegexOptions.IgnoreCase))
                {
                    var reply = activity.CreateReply(await BuildListOfVisitedRestaurantsAsync());
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (Regex.IsMatch(message, "have we been to", RegexOptions.IgnoreCase))
                {
                    // Get the parameter
                    var target = message.Replace("have we been to", "");

                    // Attempt to query firebase to see if there's a match

                    // If so, let them know when and who picked it
                    // Get the last pick and then resolve who is next from Firebase
                    var reply = activity.CreateReply("'' has not been chosen.");
                    await connector.Conversations.ReplyToActivityAsync(reply);

                }
                else if (Regex.IsMatch(message, "who's turn|who is next", RegexOptions.IgnoreCase))
                {
                    // Get the last pick and then resolve who is next from Firebase
                    var reply = activity.CreateReply("It is James' turn on ''");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    var reply = activity.CreateReply(GetDefaultResponse());
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private string GetDefaultResponse()
        {
            // Build message
            var message = new StringBuilder($"Hi, I'm Third Thursday Bot! I support the following commands: {Environment.NewLine}");
            var options = new[] {
                    "'show all' Lists all of the previous Third Thursday selections.",
                    "'have we been to '{location}' Indicates if specific restaurant has been chosen.",
                    "'who's turn is it' - Indicates who has the next selection.",
            };

            foreach (var option in options)
            {
                message.AppendLine($"- {option}");
            }

            return message.ToString();
        }

        private async Task<string> BuildListOfVisitedRestaurantsAsync()
        {
            try
            {
                var json = await _client.GetStringAsync("/Restaurants/.json");
                var restaurants = JsonConvert.DeserializeObject<Restaurant[]>(json);

                var message = new StringBuilder($"The following restaurants have been visited: {Environment.NewLine}");
                foreach (var restaurant in restaurants)
                {
                    message.AppendLine($"- '{restaurant.Location}' ({restaurant.PickedBy})");
                }

                return message.ToString();
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}