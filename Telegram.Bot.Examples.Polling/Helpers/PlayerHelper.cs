using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Examples.Polling.MethodsDB;
using Telegram.Bot.Types;

namespace Telegram.Bot.Examples.Polling.Helpers
{
    public static class PlayerHelper
    {
        public static bool IsPlaying(Message message)
        {
            if (!InteractWithPlayer.GetPlayers().Exists(x => x.User_id == message.From.Id))
                return false;
            return true;
                
        }
    }
}
