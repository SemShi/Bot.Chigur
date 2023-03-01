using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Examples.Polling.Models
{
    public class Player
    {
        public long User_id { get; set; }
        public long Chat_id { get; set; }
        public string Name { get; set; }
        public int Rank { get; set; }
        public int Health { get; set; }
        public PlayerStatistic Statistics { get; set; }
    }
}
