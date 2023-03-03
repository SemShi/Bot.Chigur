using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Examples.Polling
{
    public class CallbackParams
    {
        public string Value { get; set; }

        /// <summary>
        /// Параметр для игры в монеку
        /// </summary>
        public long WhoseCoin { get; set; }
    }
}
