using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Examples.Polling.Models;
using Telegram.Bot.Types;

namespace Telegram.Bot.Examples.Polling.MethodsDB
{
    public static class InteractWithPlayer
    {
        private static SqlConnectionStringBuilder ConnectionString = new SqlConnectionStringBuilder(@"Data Source=.\SQLEXPRESS;Initial Catalog=ChigurDB;Integrated Security=True");

        public static List<Player> GetPlayers(string conditions = "")
        {
            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                var playerList = connection.Query<Player>("select * from Players " + conditions).ToList();
                return playerList;
            }

        }

        public static void AddPlayer(Message message)
        {
            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                connection.Query<Player>($"insert into " +
                                         $"  Players(User_id, Chat_id, Name, Rank, Health, Money) " +
                                         $"values " +
                                         $"  ({message.From.Id}, {message.Chat.Id}, '{message.From.FirstName}', 0, 100, 0)");
                connection.Query<Inventory>($"insert into " +
                                            $"  Inventory(User_id, Item_id) " +
                                            $"values " +
                                            $"  {message.From.Id}, {(int)Enums.Items.Кулаки}");

                var isUserExist = connection.Query<PlayerStatistic>($"select * from PlayerStatistics where User_id={message.From.Id}").ToList();

                if (isUserExist.Count > 0) return;
                connection.Query<PlayerStatistic>($"insert into " +
                                                  $"  PlayerStatistics(User_id, Name, GamesPlayed, MaxRank, DeathCount) " +
                                                  $"values " +
                                                  $"  ({message.From.Id}, '{message.From.FirstName}', 0, 0, 0)");
            }
        }

        public static void DeletePlayer(long playerId)
        {
            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                UpdateStatPlayer(playerId, "/death");
                connection.Query<Player>($"delete from " +
                                         $"  Players " +
                                         $"where " +
                                         $"  User_id={playerId}");
            }
        }

        public static void DoDamagePlayer(long playerId, int damage)
        {
            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                connection.Query<Player>($"declare @oldHealth int " +
                                         $"set @oldHealth = (select Health from Players where User_id={playerId}) " +
                                         $"update Players " +
                                         $"set Health = @oldHealth - {damage} " +
                                         $"where User_id={playerId}");
            }
        }

        public static void UpdateRankPlayer(long playerId, int exp, string operation)
        {
            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                connection.Query<Player>($"declare @oldRank int " +
                                         $"set @oldRank = (select Rank from Players where User_id={playerId}) " +
                                         $"update Players " +
                                         $"set Rank = @oldRank {operation} {exp} " +
                                         $"where User_id={playerId}");
            }
        }

        public static void UpdateMoneyPlayer(long playerId, int count, string operation)
        {
            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                connection.Query<Player>($"declare @oldMoney int " +
                                         $"set @oldMoney = (select Money from Players where User_id={playerId}) " +
                                         $"update Players " +
                                         $"set Money = @oldMoney {operation} {count} " +
                                         $"where User_id={playerId}");
            }
        }

        public static void UpdateStatPlayer(long playerId, string method)
        {
            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                switch (method)
                {
                    case "/death":
                        {
                            var currentRecord = GetStatisticPlayer(playerId);
                            if (currentRecord.Count == 0) return;
                            var sql = $"update " +
                                        $"  PlayerStatistics " +
                                        $"set " +
                                        $"    GamesPlayed={currentRecord[0].Statistics.GamesPlayed + 1}," +
                                        $"    DeathCount={currentRecord[0].Statistics.DeathCount + 1} " +
                                        $"{(currentRecord[0].Rank > currentRecord[0].Statistics.MaxRank ? $",{currentRecord[0].Rank > currentRecord[0].Statistics.MaxRank} " : "")} " +
                                        $"where" +
                                        $"  User_id={playerId}";
                            bool s = true;
                            connection.Query<PlayerStatistic>($"update " +
                                                                $"  PlayerStatistics " +
                                                                $"set " +
                                                                $"    GamesPlayed={currentRecord[0].Statistics.GamesPlayed + 1}," +
                                                                $"    DeathCount={currentRecord[0].Statistics.DeathCount + 1} " +
                                                                $"{(currentRecord[0].Rank > currentRecord[0].Statistics.MaxRank ? $",MaxRank={currentRecord[0].Rank} " : "")} " +
                                                                $"where" +
                                                                $"  User_id={playerId}");

                            break;
                        }
                    case "/joinGame":
                        {
                            break;
                        }
                }
            }
        }

        public static List<Player> GetStatisticPlayer(long playerId)
        {
            if (!GetPlayers().Exists(x => x.User_id == playerId)) return new List<Player>();

            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                var sql = @"SELECT *
                        FROM Players p 
                        LEFT JOIN PlayerStatistics s ON s.User_id = p.User_id";

                var player = connection.Query<Player, PlayerStatistic, Player>(sql, (player, statistic) => {
                    player.Statistics = statistic;
                    return player;
                },
                splitOn: "User_id");

                return player.ToList();
            }
        }

        public static List<PlayerStatistic> GetAllStatistic()
        {
            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                return connection.Query<PlayerStatistic>("select * from PlayerStatistics").ToList();
            }
        }

        public static List<Inventory> GetPlayerInventory(long playerId)
        {
            if (!GetPlayers().Exists(x => x.User_id == playerId)) return new List<Inventory>();

            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                var sql = @"SELECT *
                        FROM Inventory i 
                        LEFT JOIN Items s ON s.Item_id = i.Item_id";

                var inventory = connection.Query<Inventory, Items, Inventory>(sql, (inv, items) =>
                {
                    inv.Items = items;
                    return inv;
                },
                splitOn: "Item_id");

                return inventory.ToList();
            }
        }
    }
}
