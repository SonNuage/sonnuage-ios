namespace SonNuage.Mobile.Services;

public static class ServerAddress
{
    /// <summary>
    /// Adresse de base du serveur, terminée par « / » : une Uri relative « api/... » se résout alors sous le chemin
    /// éventuel du serveur (https://hôte/sonnuage/api/...) au lieu de le remplacer.
    /// </summary>
    public static Uri AsBase(Uri address) => address.AbsolutePath.EndsWith('/') ? address : new Uri(address + "/");
}
