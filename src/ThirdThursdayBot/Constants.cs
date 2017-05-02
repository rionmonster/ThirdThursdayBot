namespace ThirdThursdayBot
{
    public class Constants
    {
        public const string DefaultResponseMessage =
            @"Hi, I'm Third Thursday Bot! I support the following commands:\r\n
            - 'show all' Lists all of the previous Third Thursday selections.\r\n
            - 'have we been to {restaurant}?' Indicates if specific restaurant has been chosen.\r\n
            - 'who's turn is it' - Indicates who has the next selection.\r\n";

        public const string RestaurantListingMessage =
            @"All of the following restaurants have been visited:\r\n";

        public const string NextChooserFormattingMessage =
            @"{0} has the next choice for {1}.";

        public const string UnchosenRestaurantFormattingMessage =
            @"{0} has not been chosen before. Give it a shot!";

        public const string PreviouslyChosenResturantFormattingMessage =
            @"{0} was chosen by {1} in {2}";

        public const string UnrecognizableRestaurantMessage =
            @"Sorry, I couldn't figure out what restaurant you were looking for. Try again.";

        public const string DatabaseAccessIssuesMessage =
            @"Sorry, I'm having trouble figuring that out. Try again in a little while.";
    }
}