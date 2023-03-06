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
    public static class UpdateDB
    {
        private static SqlConnectionStringBuilder ConnectionString = new SqlConnectionStringBuilder(@"Data Source=.\SQLEXPRESS;Initial Catalog=ChigurDB;Integrated Security=True");
        public static void ClearDB()
        {
            using (var connection = new SqlConnection(ConnectionString.ConnectionString))
            {
                connection.Query($"delete from Inventory; " +
                                 $"delete from Players; " +
                                 $"delete from PlayerStatistics ");
            }
        }
    }
}
