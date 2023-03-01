using Telegram.Bot.Examples.Polling.Models;
using Telegram.Bot.Types;

namespace Microsoft.Extensions.DependencyInjection.DAL;

public interface IUserDAL
{
    Task<IEnumerable<Player>> GetPlayers(string conditions ="");
    Task AddPlayer(Message message);
}
