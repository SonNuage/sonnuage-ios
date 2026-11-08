using SonNuage.UI.Services;

namespace SonNuage.Mobile.Services;

/// <summary>Preferences pour les réglages, SecureStorage (Keychain iOS) pour la session et ses jetons.</summary>
public sealed class MobileStore : IKeyValueStore
{
    private const string SecurePrefix = "sonnuage.session";

    public async Task<string?> GetAsync(string key)
    {
        if (!key.StartsWith(SecurePrefix, StringComparison.Ordinal)) return Preferences.Default.Get<string?>(key, null);
        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch (Exception ex)
        {
            // Trousseau illisible (entrée d'une autre signature de l'appli, keychain-access-groups absent après une
            // signature avec un Apple ID gratuit : voir docs/IOS.md). Le nettoyage peut lui-même échouer : on repart de zéro
            // sans jamais laisser l'exception remonter (elle tuait l'appli au démarrage).
            LogWarning(ex);
            try
            {
                SecureStorage.Default.Remove(key);
            }
            catch (Exception)
            {
                try { SecureStorage.Default.RemoveAll(); } catch (Exception) { }
            }
            return null;
        }
    }

    public async Task SetAsync(string key, string? value)
    {
        if (!key.StartsWith(SecurePrefix, StringComparison.Ordinal))
        {
            if (value is null) Preferences.Default.Remove(key);
            else Preferences.Default.Set(key, value);
            return;
        }
        try
        {
            if (value is null) SecureStorage.Default.Remove(key);
            else await SecureStorage.Default.SetAsync(key, value);
        }
        catch (Exception ex)
        {
            // Même cause que ci-dessus : on vide le stockage sécurisé et on réessaie une fois.
            LogWarning(ex);
            try { SecureStorage.Default.RemoveAll(); } catch (Exception) { }
            if (value is not null) await SecureStorage.Default.SetAsync(key, value);
        }
    }

    private static void LogWarning(Exception ex)
    {
        var message = $"SecureStorage illisible, réinitialisé : {ex}";
        System.Diagnostics.Debug.WriteLine($"SonNuage : {message}");
        CrashLog.Breadcrumbs.Add(message.Length > 300 ? message[..300] + "…" : message);
    }
}

/// <summary>
/// Adresse du serveur : celle du compte actif, ou celle saisie pour le compte en cours d'ajout tant qu'aucun compte
/// n'est actif. Pendant un ajout, l'API reste donc adressée au compte actif (lecture en arrière-plan comprise).
/// </summary>
public sealed class AccountEndpoint : IServerEndpoint
{
    private readonly AccountStore _accounts;

    public AccountEndpoint(AccountStore accounts)
    {
        _accounts = accounts;
        accounts.ActiveChanged += () => Changed?.Invoke();
        accounts.Changed += () => Changed?.Invoke();
    }

    public Uri? BaseAddress => _accounts.Active?.Server ?? _accounts.PendingServer;
    public bool IsConfigurable => true;
    public event Action? Changed;

    public Task SetAsync(Uri? address)
    {
        // Uri relative "api/..." doit se résoudre sous le chemin éventuel du serveur.
        if (address is not null && !address.AbsolutePath.EndsWith('/')) address = new Uri(address + "/");
        _accounts.SetPendingServer(address);
        return Task.CompletedTask;
    }
}

public sealed class MobilePlatform : IPlatformInfo
{
    // iOS n'expose pas de réseau « facturé » comparable : la qualité mobile n'est donc pas proposée (comme sur le web).
    public bool HasMeteredNetworkDetection => false;
    public string Name => "ios";
}
