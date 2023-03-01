using System.Data.SqlClient;
using Dapper;
using Telegram.Bot.Examples.Polling.Models;
using Telegram.Bot.Types;

namespace Microsoft.Extensions.DependencyInjection.DAL;

public class UserDAL : IUserDAL
{
    public async Task<IEnumerable<Player>> GetPlayers(string conditions = "")
    {
        using (var connection = new SqlConnection(DbHealper.Connection))
        {
            var playerList = connection.QueryAsync<Player>("select * from Players " + conditions);

            return await playerList;
        }
    }

    public Task AddPlayer(Message message)
    {
        using (var connection = new SqlConnection(ConnectionString.ConnectionString))
        {
            connection.Query<Player>($"insert into " +
                                     $"  Players(User_id, Chat_id, Name, Rank, Health) " +
                                     $"values " +
                                     $"  ({message.From.Id}, {message.Chat.Id}, '{message.From.FirstName}', 0, 100)");
        }
    }
}
