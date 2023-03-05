using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Examples.Polling.Models
{
    public class Inventory
    {
        public int PLayerItem_Id { get; set; }
        public long User_Id { get; set; }
        public int Item_Id { get; set; }

        public Items Items { get; set; }
        public ICollection<Player> Player { get; set; }

    }
}
