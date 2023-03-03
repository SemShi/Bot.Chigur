using Dapper;
using Microsoft.VisualBasic;
using System.Data.SqlClient;
using System.Net;
using System.Numerics;
using System.Threading;
using Telegram.Bot.Examples.Polling.Files;
using Telegram.Bot.Examples.Polling.Games;
using Telegram.Bot.Examples.Polling.Helpers;
using Telegram.Bot.Examples.Polling.MethodsDB;
using Telegram.Bot.Examples.Polling.Models;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private static SqlConnectionStringBuilder ConnectionString = new SqlConnectionStringBuilder(@"Data Source=.\SQLEXPRESS;Initial Catalog=ChigurDB;Integrated Security=True");
    private static string BotToken = "5721215311:AAE79QD4Esi1TxpYUl8j1BQ3RVWCVdUXAQ4";

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    /// <summary>
    /// Метод обрабатывает все сообщения, которые увидел бот.
    /// На входе проверят тип полученного сообщения.
    /// Message - обычно сообщение
    /// CallbackQuery - ответ на кнопу, созданную классом InlineKeyboardButton.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message }                         => BotOnMessageReceived(message, cancellationToken),
            //{ EditedMessage: { } message }                 => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery }             => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            //{ InlineQuery: { } inlineQuery }               => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            //{ ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            _                                                => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }

    /// <summary>
    /// Метод обрабатывает полученное сообщени, далее, если сообщение начинается с '/', при помощи конструкции switch выбирается необходимый метод.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        if (message.Text.StartsWith("/"))
        {
            var action = messageText.Replace("@UniversalSigma_bot", "").Split(' ')[0] switch
            {
                "/start"             => Start(_botClient, message, cancellationToken),
                "/iwannaplay"        => IWannaPlay(_botClient, message, cancellationToken),
                "/suicide"           => Suicide(_botClient, message, cancellationToken),
                "/mystat"            => PlayerStat(_botClient, message, cancellationToken),
                _                    => Default(_botClient, message, cancellationToken)
                //"/inline_keyboard" => SendInlineKeyboard(_botClient, message, cancellationToken),
                //"/keyboard"        => SendReplyKeyboard(_botClient, message, cancellationToken),
                //"/remove"          => RemoveKeyboard(_botClient, message, cancellationToken),
                //"/photo"           => SendFile(_botClient, message, cancellationToken),
                //"/request"         => RequestContactAndLocation(_botClient, message, cancellationToken),
                //"/inline_mode"     => StartInlineQuery(_botClient, message, cancellationToken),
                //"/throw"           => FailingHandler(_botClient, message, cancellationToken),
                //_                  => Usage(_botClient, message, cancellationToken)
            };;
            Message sentMessage = await action;
            _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
        }
        else
        {
            if(PlayerHelper.IsPlaying(message))
            {
                InteractWithPlayer.UpdateRankPlayer(message.From.Id, 1, "+");
            }
            
        }
        #region Chigur Tasks

        static async Task<Message> Start(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Приветствую! Я Сигма бот, повелитель всех сигм.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> IWannaPlay(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if(PlayerHelper.IsPlaying(message))
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
                                $"🏆Рейтинг: {newPlayer.Rank}",
                    cancellationToken: cancellationToken);
            }
            else
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Игрок \"{message.From.FirstName}\" уже добавлен в список жертв Чигура.",
                    cancellationToken: cancellationToken);

        }

        static async Task<Message> Default(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
        static async Task<Message> Suicide(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
        static async Task<Message> PlayerStat(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
                                $"\n" +
                                $"📔Общая статистика:\n" +
                                $"🌟Максимальный рейтинг: {playerStat[0].Statistics.MaxRank}\n" +
                                $"💀Кол-во смертей: {playerStat[0].Statistics.DeathCount}",
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

    /// <summary>
    /// Обработка коллбэков от нажатых кнопок
    /// </summary>
    /// <param name="callbackQuery"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        string[] data = callbackQuery.Data.Split(",");
        string value = data[0].Trim();
        string method = data[1].Trim();
        long whoseCoin = Convert.ToInt64(data[2].Trim());

        if (method.StartsWith("/"))
        {
            switch (method)
            {
                case "/flipCoin":
                    {
                        await _botClient.DeleteMessageAsync(
                            chatId: callbackQuery.Message.Chat.Id,
                            messageId: callbackQuery.Message.MessageId);

                        if(whoseCoin != callbackQuery.From.Id)
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
                            break;
                        }

                        TelegramAPI.SendGif(callbackQuery.Message.Chat.Id, GIFs.CoinFlip);
                        await Task.Delay(3000);
                        int coinSide = Convert.ToInt32(value);
                        if (Games.CoinFlip(coinSide))
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
                        break;
                    }
            }
        }


        //await _botClient.AnswerCallbackQueryAsync(
        //    callbackQueryId: callbackQuery.Id,
        //    text: $"Received {callbackQuery.Data}",
        //    cancellationToken: cancellationToken);

        //await _botClient.SendTextMessageAsync(
        //    chatId: callbackQuery.Message!.Chat.Id,
        //    text: $"Received {callbackQuery.Data}",
        //    cancellationToken: cancellationToken);
    }

    #region Inline Mode

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "1",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent("hello"))
        };

        await _botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,
            cacheTime: 0,
            isPersonal: true,
            cancellationToken: cancellationToken);
    }

    private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);

        await _botClient.SendTextMessageAsync(
            chatId: chosenInlineResult.From.Id,
            text: $"You chose result with Id: {chosenInlineResult.ResultId}",
            cancellationToken: cancellationToken);
    }

    #endregion

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}
