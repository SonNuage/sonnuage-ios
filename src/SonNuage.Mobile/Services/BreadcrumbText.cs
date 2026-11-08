namespace SonNuage.Mobile.Services;

/// <summary>Une dernière action reste courte : le rapport de plantage en garde des dizaines.</summary>
public static class BreadcrumbText
{
    public const int MaxLength = 300;

    public static string Shorten(string message) => message.Length > MaxLength ? message[..MaxLength] + "…" : message;
}
