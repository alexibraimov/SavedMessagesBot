namespace FileSaverBot.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

public static class TelegramBotClientExtensions
{
    private static string DATE_FORMAT = "yyyy-MM-dd-hh-mm-ss";
    public static async Task DownloadMessageAsync(this ITelegramBotClient botClient, Message? message, CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var replyMessage = await botClient.SendTextMessageAsync(chatId: message.Chat,
                                                                text:  "Processing...",
                                                                replyToMessageId: message.MessageId,
                                                                cancellationToken: cancellationToken);

        string? fileName = null;
        string? fileId = null;
        string? subfolder = Enum.GetName(message.Type.GetType(), message.Type);

        switch (message.Type)
        {
            case Telegram.Bot.Types.Enums.MessageType.Text:
                fileName = $"{message.Date.ToString(DATE_FORMAT)}.md";
                break;
            case Telegram.Bot.Types.Enums.MessageType.Photo:
                fileName = $"{message.Date.ToString(DATE_FORMAT)}.jpg";
                if (!string.IsNullOrWhiteSpace(message.Caption))
                {
                    fileName = $"{message.Caption}.jpg";
                }
                fileId = message.Photo?.FirstOrDefault()?.FileId;
                break;
            case Telegram.Bot.Types.Enums.MessageType.Audio:
                fileName = message.Audio?.FileName;
                fileId = message.Audio?.FileId;
                break;
            case Telegram.Bot.Types.Enums.MessageType.Video:
                fileName = message.Video?.FileName;
                fileId = message.Video?.FileId;
                break;
            case Telegram.Bot.Types.Enums.MessageType.Voice:
                fileName = $"{message.Date.ToString(DATE_FORMAT)}.ogg";
                if (string.IsNullOrWhiteSpace(message.Caption))
                {
                    fileName = $"{message.Caption}.ogg";
                }
                fileId = message.Voice?.FileId;
                break;
            case Telegram.Bot.Types.Enums.MessageType.Document:
                fileName = message.Document?.FileName;
                fileId = message.Document?.FileId;
                break;
            case Telegram.Bot.Types.Enums.MessageType.Sticker:
                fileName = $"{message.Date.ToString(DATE_FORMAT)}.tgs";
                if (!string.IsNullOrWhiteSpace(message.Sticker?.SetName))
                {
                    fileName = $"{message.Sticker.SetName}.tgs";
                }
                if (!string.IsNullOrWhiteSpace(message.Caption))
                {
                    fileName = $"{message.Caption}.tgs";
                }
                fileId = message.Sticker?.FileId;
                break;
            case Telegram.Bot.Types.Enums.MessageType.VideoNote:
                subfolder = "Video";
                fileName = $"{message.Date.ToString(DATE_FORMAT)}.mp4";
                if (string.IsNullOrWhiteSpace(message.Caption))
                {
                    fileName = $"{message.Caption}.mp4";
                }
                fileId = message.VideoNote?.FileId;
                break;
            default:
                {
                    await botClient.EditMessageTextAsync(chatId: message.Chat,
                                                         text: Settings.GetMessage("NotSupportedException"),
                                                         messageId: replyMessage.MessageId,
                                                         cancellationToken: cancellationToken);
                }
                return;
        }

        var folderPath = $"{Settings.BASE_FOLDER}\\{subfolder}";
        var fullpath = $"{folderPath}\\{fileName}";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        if (fileId != null)
        {
            try
            {
                int index = 1;
                var format = fullpath.Split('.').LastOrDefault();
                while (System.IO.File.Exists(fullpath))
                {
                    fullpath = $"{folderPath}\\{fileName?.Replace($".{format}", $" ({index})")}.{format}";
                    index++;
                }
                var file = await botClient.GetFileAsync(fileId);
                var filepath = file.FilePath;
                if (string.IsNullOrEmpty(filepath))
                {
                    throw new ArgumentNullException(nameof(filepath));
                }
                using (var fstream = new FileStream(fullpath, FileMode.Create))
                {
                    await botClient.DownloadFileAsync(filePath: filepath,
                                                      destination: fstream,
                                                      cancellationToken: cancellationToken);
                }

                await botClient.EditMessageTextAsync(chatId: message.Chat,
                                                     text: fullpath,
                                                     messageId: replyMessage.MessageId,
                                                     cancellationToken: cancellationToken);
                replyMessage = null;
            }
            catch (Exception)
            {
                await botClient.EditMessageTextAsync(chatId: message.Chat,
                                                     text: Settings.GetMessage("OopsSomethingWentWrongPleaseRetryThisAction"),
                                                     messageId: replyMessage.MessageId,
                                                     cancellationToken: cancellationToken);
            }
        }
        else
        {
            try
            {
                using (var fstream = new FileStream(fullpath, FileMode.Create))
                {
                    byte[] buffer = Encoding.Default.GetBytes(message?.Text!);
                    await fstream.WriteAsync(buffer, 0, buffer.Length);
                }

                await botClient.EditMessageTextAsync(chatId: message.Chat,
                                                     text: fullpath,
                                                     messageId: replyMessage.MessageId,
                                                     cancellationToken: cancellationToken);
            }
            catch (Exception)
            {
                await botClient.EditMessageTextAsync(chatId: message.Chat,
                                                     text: Settings.GetMessage("OopsSomethingWentWrongPleaseRetryThisAction"),
                                                     messageId: replyMessage.MessageId, cancellationToken: cancellationToken);
            }
        }
    }
}
