using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.Examples.Polling.Games
{
    public static class Games
    {
        /// <summary>
        /// Когда Чигур предлагает выбрать сторону, метод решает выживет игрок или нет.
        /// </summary>
        /// <param name="CoinSide">Сторона монетки. Орел - 0, Решка - 1</param>
        /// <returns>True - сохраняет жизнь, False - забирает жизнь</returns>
        public static bool CoinFlip(int coinSide)
        {
            var rnd = new Random();
            if (coinSide == rnd.Next(0, 2))
                return true;
            else
                return false;
        }
    }
}
