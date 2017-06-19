# ThirdThursdayBot

## What is "Third Thursday" and what is this project?

The reasoning for this bot surrounds an event that my friends and I have been doing for a few years now that we call "Third Thursday". Third Thursday is a monthly gathering, as you might expect on the third Thursday of each month and it works as follows:

 - Each month a different member of the group gets to choose where we all eat.
 - No restaurants can be repeated (until all local areas are exhausted).
 - The restaurant must serve alcohol (and obviously food).

Fairly simple right? Well, after doing this for over two years, it can be tough to remember where we have eaten previously, whose choice it is this month, and when the next day is, so I thought that I could build a bot to handle this for us.

## Goals for this project

 - **Trivial Set-up** - Since this was a small side project, I didn't want to invest a ton of time/effort into infrastructure or scaling concerns.
 - **Basic Question / Answer Support** - Although Azure has quite a few incredible Cognitive Services features, I just wanted the bot to display a list of commands that users could enter and receive quick responses.
 - **Web / Skype / Text Support** - I wanted my friends and members of this group to be able to easily access the bot from wherever they were. On a computer, check a site, on your phone, send it a text, etc.
 
## In Action

You can [visit Third Thursday Bot in action here](http://thirdthursdaybot.azurewebsites.net/) and see a basic example of some of the interactions that you can perform with it below:

![listing restaurants](http://rion.io/content/images/2017/05/listing-restaurants.gif)

## Supported Commands

At present, Third Thursday Bot supports the following commands:

- **show all** - Lists all of the previous Third Thursday selections.
- **have we been to {restaurant}?** - Indicates if specific restaurant has been chosen.
- **who’s next** - Indicates who has the next selection.
- **recommendation** - Get a random recommendation in the area.

## Configuring Your Own

To configure a Third Thursday of your own, you would just need to do the following:

 - **Create a Microsoft Bot Framework Account** - This bot relies on [the Microsoft Bot Framework](https://dev.botframework.com/) for its supported integrations, which can make getting started and actually using a bot a breeze.
 - **Set up Firebase** - This example uses [Firebase](https://firebase.google.com/) as a database, which can be set up in a matter of minutes.
 - **Configure Twilio (Optional)** - If you want to take advantage of SMS messaging to/from the bot, you'll need to create and configure a [Twilio](https://www.twilio.com/) account.
 
With regards to Firebase, your schema is going to look something like this (if you are going to be reproducing an example similar to this one):
 
 - **Members** - Array containing members of the group (e.g. ['Dwight', 'Michael', 'Jim', 'Pam', ... ])
 - **Restaurants** - Array containing previously visited Restaurant objects, which look have the following properties:
     - **Date** - A string representing the date a given restaurant was visited.
     - **Location** - A string representing the name of the restaurant chosen.
     - **PickedBy** - A string with the name of the group member that picked the restaurant.

Finally, this example is hosted in Azure and uses it to define all of the available environmental variables for your private keys, HTTP endpoint locations, and more so that they can be 
read in and used by the bot at runtime:

![Azure Setup](https://rionghost.azurewebsites.net/content/images/2017/06/AppSettingsInAzure.PNG)

You can read more on this process within the following blog posts:
 
 - [How the Microsoft Bot Framework Changed Where My Friends and I Eat: Part 1](http://rion.io/2017/05/11/how-the-microsoft-bot-framework-changed-where-my-friends-and-i-eat-part-1/)
 - [How the Microsoft Bot Framework Changed Where My Friends and I Eat: Part 2](http://rion.io/2017/06/19/how-the-microsoft-bot-framework-changed-where-my-friends-and-i-eat-part-2/)
