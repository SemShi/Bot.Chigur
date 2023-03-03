using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Examples.Polling.Helpers
{
    public static class TelegramAPI
    {
        private static string BotToken = "5721215311:AAE79QD4Esi1TxpYUl8j1BQ3RVWCVdUXAQ4";
        public static void SendGif(long ChatId, string uriGif)
        {
            string uri =
                $"https://api.telegram.org/bot{BotToken}/sendVideo?chat_id={ChatId}&video={uriGif}";
            var web = new WebClient();
            var bytes = web.DownloadData(uri);
        }
    }
}
