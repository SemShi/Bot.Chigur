using Dapper;
using Microsoft.VisualBasic;
using System.Data.SqlClient;
using System.Net;
using System.Numerics;
using System.Threading;
using Telegram.Bot.Examples.Polling;
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
                "/start"             => Commands.Start(_botClient, message, cancellationToken),
                "/iwannaplay"        => Commands.IWannaPlay(_botClient, message, cancellationToken),
                "/suicide"           => Commands.Suicide(_botClient, message, cancellationToken),
                "/mystat"            => Commands.PlayerStat(_botClient, message, cancellationToken),
                "/delallfromdb"      => Commands.ClearDatabase(_botClient, message, cancellationToken),
                "/addplayer"         => Commands.AddPlayerByAdmin(_botClient, message, cancellationToken),
                "/inventory"         => Commands.PlayerInventory(_botClient, message, cancellationToken),
                _                    => Commands.Default(_botClient, message, cancellationToken)
            };
            Message sentMessage = await action;
            _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
        }
        else
        {
            if(!PlayerHelper.IsPlaying(message))
                InteractWithPlayer.UpdateRankPlayer(message.From.Id, 1, "+");
        }
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

        if (method.StartsWith("/"))
        {
            switch (method)
            {
                case "/flipCoin":
                    {
                        long whoseCoin = Convert.ToInt64(data[2].Trim());
                        await Callbacks.FlipCoin(_botClient, new CallbackParams { Value= value, WhoseCoin = whoseCoin }, callbackQuery, cancellationToken);
                        break;
                    }
                case "/addPlayer":
                    {
                        long UserId = Convert.ToInt64(value);
                        long ChatId = Convert.ToInt64(data[2].Trim());
                        string Name = data[3].Trim();
                        var playerParams = new Player() { User_id = UserId, Chat_id = ChatId, Name = Name };
                        await Callbacks.AddPlayer(_botClient, playerParams, cancellationToken);
                        break;
                    }
            }
        }
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
    #region Обработчики ошибок
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
    #endregion
}
