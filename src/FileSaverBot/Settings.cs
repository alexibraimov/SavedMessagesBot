namespace FileSaverBot;

using System;
using System.IO;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class Settings
{
    private static SettingsModel? settings;
    private static Dictionary<string, string> DEFAULT_LOCALE = new Dictionary<string, string>()
    {
        ["/stop"] = "Bye, bye!",
        ["/start"] = "Welcome to! \nSend me files, photos, stickers or text. I will save them on your computer.",
        ["/help"] = "Send me files, photos, stickers or text. I will save them on your computer.",
        ["OopsSomethingWentWrongPleaseRetryThisAction"] = "Oops, something went wrong. Please retry this action.",
        ["ArgumentNullException"] = "Oops, something went wrong. Please retry this action.",
        ["NotSupportedException"] = "Sorry, this message type is not supported.",
        ["AccessDeniedException"] = "Sorry, you are denied access."
    };

    static Settings()
    {
        try
        {
            settings = JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(
                Path.Combine(Environment.CurrentDirectory, "globalSettings.json")));
        }
        catch (Exception ex)
        {

        }
    }

    public static string BASE_FOLDER => settings?.BaseFoldder == null ? "D:\\SavedMessages" : settings.BaseFoldder;
    public static string TOKEN => settings?.Token == null ? string.Empty : settings.Token;
    public static string[] USERS => settings?.Users == null ? Array.Empty<string>() : settings.Users;

    public static string GetMessage(string key)
    {
        if (settings?.Locale != null && settings.Locale.ContainsKey(key))
        {
            return settings.Locale[key];
        }
        return DEFAULT_LOCALE[key];
    }

    private class SettingsModel
    {
        [JsonPropertyName("baseFolder")]
        public string? BaseFoldder { get; set; }
        [JsonPropertyName("token")]
        public string? Token { get; set; }
        [JsonPropertyName("users")]
        public string[]? Users { get; set; }
        [JsonPropertyName("locale")]
        public Dictionary<string, string>? Locale { get; set; }
    }
}

