using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot.Examples.Polling.Files;
using Telegram.Bot.Examples.Polling.Helpers;
using Telegram.Bot.Examples.Polling.MethodsDB;
using Telegram.Bot.Types.Enums;
using System.Data.SqlClient;
using Telegram.Bot.Examples.Polling.Models;

namespace Telegram.Bot.Examples.Polling
{
    public static class Commands
    {
        private const string AdminId = "367104170";

       public static async Task<Message> Start(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Приветствую! Я Сигма бот, повелитель всех сигм.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        public static async Task<Message> IWannaPlay(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (PlayerHelper.IsPlaying(message))
            {
                InteractWithPlayer.AddPlayer(message);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"❗Игрок \"{message.From.FirstName}\" добавлен в список жертв Чигура!",
                    cancellationToken: cancellationToken);

                var newPlayer = InteractWithPlayer.GetPlayers($"where User_id={message.From.Id}")[0];
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"📝Статистика игрока {message.From.FirstName}:\n" +
                                $"🤠Имя: {newPlayer.Name}\n" +
                                $"💊Здоровье: {newPlayer.Health}\n" +
                                $"💰Шиллинги: {newPlayer.Money}\n" +
                                $"🏆Рейтинг: {newPlayer.Rank}",
                    cancellationToken: cancellationToken);
            }
            else
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Игрок \"{message.From.FirstName}\" уже добавлен в список жертв Чигура.",
                    cancellationToken: cancellationToken);

        }

        public static async Task AddPlayerByAdmin(ITelegramBotClient botClient, Player playerParams, CancellationToken cancellationToken)
        {
            Message message = new Message();
            message.From = new User();
            message.Chat = new Chat();
            message.From.Id = playerParams.User_id;
            message.From.FirstName = playerParams.Name;
            message.Chat.Id= playerParams.Chat_id;
            await IWannaPlay(botClient, message, cancellationToken);
        }

        public static async Task<Message> Default(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (PlayerHelper.IsPlaying(message))
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"\"{message.From.FirstName}\", ты не в игре. Используй команду /iwannaplay, попробуй обыграть повелителя Сигм!",
                    cancellationToken: cancellationToken);
            }

            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(text: "Орёл", callbackData: $"0,/flipCoin,{message.From.Id}"),
                    InlineKeyboardButton.WithCallbackData(text: "Решка", callbackData: $"1,/flipCoin,{message.From.Id}"),
                },
            });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"\"{message.From.FirstName}\", мне не понравился твой взгляд. Теперь ты вынужден бросить монету. Какую сторону выберешь?",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        public static async Task<Message> Suicide(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (PlayerHelper.IsPlaying(message))
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"\"{message.From.FirstName}\", ты не в игре. Используй команду /iwannaplay, попробуй обыграть повелителя Сигм!",
                    cancellationToken: cancellationToken);
            }
            InteractWithPlayer.DeletePlayer(message.From.Id);
            TelegramAPI.SendGif(message.Chat.Id, GIFs.Suicide);
            await Task.Delay(2000);
            return await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"Игрок {message.From.FirstName} обоссан и слит.💀",
                                cancellationToken: cancellationToken);
        }

        public static async Task<Message> PlayerStat(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var playerStat = InteractWithPlayer.GetStatisticPlayer(message.From.Id);
            if (playerStat.Count == 0)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"\"{message.From.FirstName}\", ты не в игре. Используй команду /iwannaplay, попробуй обыграть повелителя Сигм!",
                    cancellationToken: cancellationToken);
            }
            return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"📝Текущая статистика игрока {message.From.FirstName}:\n" +
                                $"🤠Имя: {playerStat[0].Name}\n" +
                                $"💊Здоровье: {playerStat[0].Health}\n" +
                                $"🏆Рейтинг: {playerStat[0].Rank}\n" +
                                $"💰Шиллинги: {playerStat[0].Money}\n" +
                                $"\n" +
                                $"📔Общая статистика:\n" +
                                $"🌟Максимальный рейтинг: {playerStat[0].Statistics.MaxRank}\n" +
                                $"💀Кол-во смертей: {playerStat[0].Statistics.DeathCount}",
                    cancellationToken: cancellationToken);
        }

        public static async Task<Message> PlayerInventory(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var playerInventory = InteractWithPlayer.GetPlayerInventory(message.From.Id);
            if (playerInventory.Count == 0)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"\"{message.From.FirstName}\", ты не в игре. Используй команду /iwannaplay, попробуй обыграть повелителя Сигм!",
                    cancellationToken: cancellationToken);
            }
            var msgInv = new StringBuilder();
            msgInv.Append($"🎒Инвентарь игрока \"{message.From.FirstName}\":\n");
            foreach(var item in playerInventory )
            {
                msgInv.Append($"— {item.Items.Name}\n");
            }
            return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: msgInv.ToString(),
                    cancellationToken: cancellationToken);
        }

        public static async Task<Message> StartAttacking(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, Inventory Item = null)
        {
            if (PlayerHelper.IsPlaying(message))
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"\"{message.From.FirstName}\", ты не в игре. Используй команду /iwannaplay, попробуй обыграть повелителя Сигм!",
                    cancellationToken: cancellationToken);
            }

            if(Item == null)
            {
                var items = InteractWithPlayer.GetPlayerInventory(message.From.Id);
                InlineKeyboardButton[] buttonsInv = new InlineKeyboardButton[items.Count];

                int o = 0;
                foreach (var item in items)
                {
                    buttonsInv[o] = InlineKeyboardButton.WithCallbackData(text: item.Items.Name, callbackData: $"{item.PLayerItem_Id},/selectItem,{item.Item_Id}");
                    o++;
                }

                InlineKeyboardMarkup inlineKeyboardInv = new(Helpers.Extensions.GetButtons(items, buttonsInv));

                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Выберите предмет:",
                    replyMarkup: inlineKeyboardInv,
                    cancellationToken: cancellationToken);
            }
            
            var playersInGame = InteractWithPlayer.GetPlayers();
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[playersInGame.Count];

            int i = 0;
            foreach (var player in playersInGame)
            {
                buttons[i] = InlineKeyboardButton.WithCallbackData(text: player.Name ?? "Unknown", callbackData: $"{player.User_id},/attack,{player.User_id},{player.Name},{Item.Item_Id}");
                i++;
            }

            InlineKeyboardMarkup inlineKeyboard = new(Helpers.Extensions.GetButtons(playersInGame, buttons));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Выберите игрока для атаки:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);

        }

        public static async Task<Message> ContinueAttacking(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, Player player, int itemId)
        {
            await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"\"{message.From.FirstName}\" атакует игрока \"{player.Name}\" предметом \"{InteractWithPlayer.GetItemName(itemId)}\"!",
                    cancellationToken: cancellationToken);
            InteractWithPlayer.DoDamagePlayer(player.User_id, "-", itemId, out bool isAlive, out int currentHP);

            if (isAlive)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"У игрока \"{player.Name}\" осталось \"{currentHP}\" ед. здоровья!",
                    cancellationToken: cancellationToken);
            }
            return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Игрок \"{player.Name}\" обоссан и слит.💀",
                    cancellationToken: cancellationToken);

        }

        #region Админские команды
        public static async Task<Message> ClearDatabase(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.From.Id.ToString() != AdminId)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"У вас нет прав к этой команде.",
                    cancellationToken: cancellationToken);
            }

            UpdateDB.ClearDB();
            return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"База очищена.",
                    cancellationToken: cancellationToken);

        }

        public static async Task<Message> AddPlayerByAdmin(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.From.Id.ToString() != AdminId)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"У вас нет прав к этой команде.",
                    cancellationToken: cancellationToken);
            }

            var existingPlayers = InteractWithPlayer.GetAllStatistic();
            if (existingPlayers.Count == 0)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Список игроков пуст.",
                    cancellationToken: cancellationToken);
            }

            #region Формируем кнопочки
            InlineKeyboardButton[] buttons = new InlineKeyboardButton[existingPlayers.Count];

            int i = 0;
            foreach (var player in existingPlayers)
            {
                buttons[i] = InlineKeyboardButton.WithCallbackData(text: player.Name ?? "Unknown", callbackData: $"{player.User_id},/addPlayer,{message.Chat.Id},{player.Name ?? "Unknown"}");
                i++;
            }
            #endregion

            InlineKeyboardMarkup inlineKeyboard = new(Helpers.Extensions.GetButtons(existingPlayers, buttons));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Выберите игрока:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        #endregion
        #region Примеры отправки сообщений
        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler

        static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                chatId: message.Chat.Id,
                chatAction: ChatAction.Typing,
                cancellationToken: cancellationToken);

            // Simulate longer running task
            await Task.Delay(500, cancellationToken);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    },
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                })
            {
                ResizeKeyboard = true
            };

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Removing keyboard",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> SendFile(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                message.Chat.Id,
                ChatAction.UploadPhoto,
                cancellationToken: cancellationToken);

            const string filePath = "Files/tux.png";
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            return await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputFile(fileStream, fileName),
                caption: "Nice Picture",
                cancellationToken: cancellationToken);
        }

        static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup RequestReplyKeyboard = new(
                new[]
                {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Who or Where are you?",
                replyMarkup: RequestReplyKeyboard,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            const string usage = "Usage:\n" +
                                 "/inline_keyboard - send inline keyboard\n" +
                                 "/keyboard    - send custom keyboard\n" +
                                 "/remove      - remove custom keyboard\n" +
                                 "/photo       - send a photo\n" +
                                 "/request     - request location or contact\n" +
                                 "/inline_mode - send keyboard with Inline Query";

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> StartInlineQuery(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode"));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Press the button to start Inline Query",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable IDE0060 // Remove unused parameter
        static Task<Message> FailingHandler(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            throw new IndexOutOfRangeException();
        }
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter.
        #endregion
    }
}
