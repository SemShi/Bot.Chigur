using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Examples.Polling.Models
{
    public class Items
    {
        public int Item_Id { get; set; }
        public int Type_id { get; set; }
        public string Name { get; set; }
        public int? Damage { get; set; }
        public int? Other { get; set; }

        public ICollection<Inventory> PlayerInventory { get; set; }
    }
}
