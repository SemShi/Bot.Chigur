using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Examples.Polling.Models
{
    public class PlayerStatistic
    {
        public long User_id { get; set; }
        public int GamesPlayed { get; set; }
        public int MaxRank { get; set; }
        public int DeathCount { get; set; }
        public ICollection<Player> Players { get; set; }
    }
}
