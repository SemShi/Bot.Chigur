using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Examples.Polling.Helpers
{
    public static class Extensions
    {
        public static bool IsIndexExist<T>(T[] obj, int index)
        {
            try
            {
                if (obj[index] == null) { }
                return true;
            }
            catch { return false; }
        }
    }
}
