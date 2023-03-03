using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Examples.Polling.Files;
using Telegram.Bot.Examples.Polling.Helpers;
using Telegram.Bot.Examples.Polling.MethodsDB;
using Telegram.Bot.Types;
using Telegram.Bot.Examples.Polling.Games;

namespace Telegram.Bot.Examples.Polling
{
    public static class Callbacks
    {
        public static async Task FlipCoin(ITelegramBotClient _botClient, CallbackParams parameters, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botClient.DeleteMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            messageId: callbackQuery.Message.MessageId);

            if (parameters.WhoseCoin != callbackQuery.From.Id)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: $"\"{callbackQuery.From.FirstName}\", не стоит вмешиваться не в свое дело.",
                    cancellationToken: cancellationToken);
                await Task.Delay(2000);
                await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                    text: "Сэр, пожалуйста, не двигайтесь.",
                    cancellationToken: cancellationToken);
                TelegramAPI.SendGif(callbackQuery.Message.Chat.Id, GIFs.PleaseDontMove);
                await Task.Delay(2000);

                InteractWithPlayer.DeletePlayer(callbackQuery.From.Id);

                //TODO Эту хуйню надо убрать, т.к. в случае смерти игрока должно вызываться событие с последующим уведомлением
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: $"Игрок {callbackQuery.From.FirstName} убит.💀",
                    cancellationToken: cancellationToken);
                return;
            }

            TelegramAPI.SendGif(callbackQuery.Message.Chat.Id, GIFs.CoinFlip);
            await Task.Delay(3000);
            int coinSide = Convert.ToInt32(parameters.Value);
            if (Games.Games.CoinFlip(coinSide))
            {
                await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                    text: "Желаю хорошего дня.🙌",
                    cancellationToken: cancellationToken);
                TelegramAPI.SendGif(callbackQuery.Message.Chat.Id, GIFs.PlayerWin);
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                text: "Мне жаль...🔪",
                    cancellationToken: cancellationToken);
                await Task.Delay(1000);
                TelegramAPI.SendGif(callbackQuery.Message.Chat.Id, GIFs.PlayerLose);
                InteractWithPlayer.DeletePlayer(callbackQuery.From.Id);

                //TODO Эту хуйню надо убрать, т.к. в случае смерти игрока должно вызываться событие
                await _botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: $"Игрок {callbackQuery.From.FirstName} убит.💀",
                    cancellationToken: cancellationToken);
            }
        }

        #region Примеры ответов на коллбеки
        static async Task CallbackAnswer(ITelegramBotClient _botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}",
                cancellationToken: cancellationToken);

            await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                text: $"Received {callbackQuery.Data}",
                cancellationToken: cancellationToken);
        }
        #endregion
    }
}
