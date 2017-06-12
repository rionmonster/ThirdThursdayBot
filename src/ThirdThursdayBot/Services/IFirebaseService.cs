using System.Threading.Tasks;
using ThirdThursdayBot.Models;

namespace ThirdThursdayBot.Services
{
    interface IFirebaseService
    {
        Task<Restaurant[]> GetAllVisitedRestaurantsAsync();
        Task<Restaurant> GetLastVisitedRestaurantAsync();
        Task<string> GetPreviouslyVisitedRestaurantsMessageAsync();
        Task<string[]> GetAllMembersAsync();
    }
}
