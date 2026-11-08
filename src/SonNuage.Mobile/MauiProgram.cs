using SonNuage.Mobile.Services;
using SonNuage.UI;
using SonNuage.UI.Services;
using Microsoft.Extensions.Logging;

namespace SonNuage.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddMauiBlazorWebView();
        builder.Logging.AddProvider(new CrashLoggerProvider());
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        // Le journal de plantage et l'interface partagent les mêmes dernières actions.
        builder.Services.AddSingleton(CrashLog.Breadcrumbs);
        builder.Services.AddSingleton<IKeyValueStore, MobileStore>();
        builder.Services.AddSingleton<IServerEndpoint, AccountEndpoint>();
        builder.Services.AddSingleton<IPlatformInfo, MobilePlatform>();
        // Lecteur de la webview (audio HTML + MediaSession), comme l'appli de bureau. PageJsRuntime donne au singleton
        // l'IJSRuntime de la page, que Blazor ne crée qu'avec la webview.
        builder.Services.AddSingleton<PageJsRuntime>();
        builder.Services.AddSingleton<IPlayer>(sp => new WebPlayer(
            sp.GetRequiredService<PageJsRuntime>(), sp.GetRequiredService<ApiClient>(), sp.GetRequiredService<PlaybackSettings>(),
            sp.GetRequiredService<IKeyValueStore>(), sp.GetRequiredService<AccountStore>()));
        builder.Services.AddSonNuageUi(multiAccount: true);

        var app = builder.Build();
        // Comptes (migration comprise) et réglages de lecture du compte actif sont chargés avant toute interface. Hors
        // du thread principal : SecureStorage y reviendrait et l'attente synchrone le bloquerait.
        var accounts = app.Services.GetRequiredService<AccountStore>();
        var settings = app.Services.GetRequiredService<PlaybackSettings>();
        Task.Run(async () =>
        {
            await accounts.LoadAsync();
            await settings.LoadAsync();
        }).GetAwaiter().GetResult();
        return app;
    }

    /// <summary>Application minimale qui affiche le rapport quand le démarrage normal a échoué.</summary>
    public static MauiApp CreateCrashApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<CrashApp>();
        return builder.Build();
    }
}
