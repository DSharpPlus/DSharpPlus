using System.Text.RegularExpressions;

namespace DSharpPlus.Logging;

internal static partial class AnonymizationUtilities
{
    [GeneratedRegex("\\\"token\\\":\\\"[a-zA-Z0-9\\-\\. ]+\\\"")]
    private static partial Regex GetJsonEncodedTokenRegex();

    [GeneratedRegex("\\/webhooks\\/[0-9]+\\/[a-zA-Z0-9\\-\\. ]+\\/")]
    private static partial Regex GetWebhookPathRegex();

    public static string AnonymizeTokens(string input)
    {
        string intermediate = GetJsonEncodedTokenRegex().Replace(input, "\"token\":\"<redacted>\"");
        intermediate = GetWebhookPathRegex().Replace(intermediate, "/webhooks/<id>/<token>");
        return intermediate;
    }

    // --------------------------------------------------------------------------------------------------

    [GeneratedRegex("\\\"[0-9]{17,22}\\\"")]
    private static partial Regex GetSnowflakeRegex();

    [GeneratedRegex("\\\"content\\\":\\\"[^\"]+\\\"")]
    private static partial Regex GetMessageContentRegex();

    [GeneratedRegex("\\\"username\\\":\\\"[a-zA-Z0-9\\.\\-_]{3,32}\\\"")]
    private static partial Regex GetUsernameRegex();

    public static string AnonymizeContents(string input)
    {
        string intermediate = GetSnowflakeRegex().Replace(input, "\"<redacted>\"");
        intermediate = GetMessageContentRegex().Replace(intermediate, "\"content\":\"<redacted>\"");
        intermediate = GetUsernameRegex().Replace(intermediate, "\"username\":\"<redacted>\"");
        return intermediate;
    }
}
