using FileSaverBot;
using FileSaverBot.Exceptions;
using FileSaverBot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


if (string.IsNullOrEmpty(Settings.TOKEN))
{
    Console.WriteLine("Application configured incorrectly");
    Console.ReadLine();
    return;
}

var supportCommands = new HashSet<string>()
{
    "/start", "/stop", "/help"
};

using var cts = new CancellationTokenSource();

var botClient = new TelegramBotClient(Settings.TOKEN);
var receiverOptions = new ReceiverOptions()
{
    AllowedUpdates = new UpdateType[]
    {
        UpdateType.Message
    }
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    try
    {
        if (update == null || update.Message == null)
        {
            throw new ArgumentNullException(Settings.GetMessage(nameof(ArgumentNullException)));
        }

        if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message)
        {
            throw new NotSupportedException(Settings.GetMessage(nameof(NotSupportedException)));
        }

        if (!Settings.USERS.Contains(update.Message.Chat.Username))
        {
            throw new AccessDeniedException(Settings.GetMessage(nameof(AccessDeniedException)));
        }

        Console.WriteLine($"<---\n{update.Message.Date}\n@{update.Message.Chat.Username}\nChatId:{update.Message.Chat.Id}\nType:{update.Message.Type}\nMessage: {update.Message.Text}\nCaption: {update.Message.Caption}\n--->");

        if (update?.Message?.Text != null && supportCommands.Contains(update.Message.Text))
        {
            _ = await botClient.SendTextMessageAsync(chatId: update.Message.Chat,
                                                     text: Settings.GetMessage(update.Message.Text),
                                                     cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.DownloadMessageAsync(message: update?.Message,
                                                 cancellationToken: cancellationToken);
        }
    }
    catch (Exception ex)
    {
        if (update?.Message != null)
        {
            _ = await botClient.SendTextMessageAsync(chatId: update.Message.Chat,
                                                     text: ex.Message,
                                                     replyToMessageId: update.Message.MessageId,
                                                     cancellationToken: cancellationToken);
        }
        Console.WriteLine(ex);
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}
