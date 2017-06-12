using System.Threading.Tasks;
using ThirdThursdayBot.Models;

namespace ThirdThursdayBot.Services
{
    public interface IYelpService
    {
        Task<YelpBusiness> GetRandomUnvisitedRestaurantAsync(Restaurant[] previouslyVisitedRestaurants);
    }
}
