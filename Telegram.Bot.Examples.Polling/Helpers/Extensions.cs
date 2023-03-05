using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

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

        public static InlineKeyboardButton[][] GetButtons<T>(List<T> list, InlineKeyboardButton[] buttons)
        {
            bool IsCountLinesEven = list.Count % 2 == 0 ? true : false;
            int lines;
            if (list.Count == 1 || list.Count == 2) lines = 1;
            else if (list.Count == 3 || list.Count == 4) lines = 2;
            else lines = list.Count / 2 + (IsCountLinesEven ? 1 : 0);

            InlineKeyboardButton[][] inlineButtons = new InlineKeyboardButton[lines][];
            int i = 0;
            for (int j = 0; j < lines; j++)
            {
                if (Helpers.Extensions.IsIndexExist(buttons, i + 2))
                {
                    inlineButtons[j] = new InlineKeyboardButton[2]
                    {
                        buttons[i],
                        buttons[i+1]
                    };
                    i += 2;
                }
                else if (Helpers.Extensions.IsIndexExist(buttons, i + 1))
                {
                    inlineButtons[j] = new InlineKeyboardButton[1]
                    {
                        buttons[i]
                    };
                }
                else if (Helpers.Extensions.IsIndexExist(buttons, i))
                {
                    inlineButtons[j] = new InlineKeyboardButton[1]
                    {
                        buttons[i]
                    };
                }
                else break;
            }
            return inlineButtons;
        }
    }
}
