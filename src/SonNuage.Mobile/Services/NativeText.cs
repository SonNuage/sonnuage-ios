using SonNuage.Contracts.Localization;
using SonNuage.UI.Services;

namespace SonNuage.Mobile.Services;

/// <summary>
/// Textes de la partie native (écran de plantage), hors Blazor : mêmes fichiers de langue,
/// dans la langue de l'interface si elle a démarré, sinon celle de l'appareil.
/// </summary>
public static class NativeText
{
    public static string Get(string key, string fallback)
    {
        try
        {
            var t = Translations.Instance;
            var language = Localizer.Current?.Language ?? t.Resolve(DeviceLanguageTag());
            return t.Has(language, key) || t.Has("en", key) ? t.Get(language, key) : fallback;
        }
        catch (Exception)
        {
            // L'écran de plantage doit s'afficher quoi qu'il arrive, même si les traductions sont illisibles.
            return fallback;
        }
    }

    private static string DeviceLanguageTag() =>
        System.Globalization.CultureInfo.CurrentUICulture.Name;
}
