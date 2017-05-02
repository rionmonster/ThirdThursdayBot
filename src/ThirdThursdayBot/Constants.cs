﻿using System;

namespace ThirdThursdayBot
{
    public class Constants
    {
        public readonly static string DefaultResponseMessage =
            $"Hi, I'm Third Thursday Bot! I support the following commands: {Environment.NewLine}" +
            $"- 'show all' Lists all of the previous Third Thursday selections. {Environment.NewLine}" +
            $"- 'have we been to {{restaurant}}?' Indicates if specific restaurant has been chosen. {Environment.NewLine}" +
            $"- 'who's next' - Indicates who has the next selection. {Environment.NewLine}";

        public readonly static string RestaurantListingMessage =
            $"All of the following restaurants have been visited: {Environment.NewLine}";

        public const string NextChooserFormattingMessage =
            @"{0} has the next choice for {1:MMMM}.";

        public const string UnchosenRestaurantFormattingMessage =
            @"{0} has not been chosen before. Give it a shot!";

        public const string PreviouslyChosenResturantFormattingMessage =
            @"{0} was chosen by {1} in {2:MMMM} of {2:yyyy}";

        public const string UnrecognizableRestaurantMessage =
            @"Sorry, I couldn't figure out what restaurant you were looking for. Try again.";

        public const string DatabaseAccessIssuesMessage =
            @"Sorry, I'm having trouble figuring that out. Try again in a little while.";
    }
}