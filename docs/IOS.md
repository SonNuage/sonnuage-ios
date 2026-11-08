# SonNuage iOS

Cible `net10.0-ios` du même projet MAUI Blazor Hybrid que l'appli Android : les pages viennent de `SonNuage.UI`.

## Ce qui est partagé, ce qui diffère

| | Android | iOS |
|---|---|---|
| Interface | `SonNuage.UI` (Blazor) | idem |
| Comptes, réglages, plantages | `MobileStore`, `AccountStore`, `CrashLog` | idem (appels natifs derrière `#if ANDROID`) |
| Lecteur | service Media3 natif (`Playback/`, `AndroidPlayer`) | `WebPlayer` de `SonNuage.UI` : balise audio + MediaSession dans la webview, comme l'appli de bureau |
| Écran verrouillé / arrière-plan | notification Media3 | `AVAudioSession` en catégorie playback + `UIBackgroundModes=audio` ; contrôles via `navigator.mediaSession` |
| Voiture | Android Auto | **non fait** : CarPlay exige une autorisation d'Apple (entitlement) qu'on ne peut pas obtenir avec un Apple ID gratuit |
| Qualité réseau mobile | détection réseau facturé | non proposée (iOS n'expose pas l'équivalent) |
| Cache audio disque 1 Go, pré-chargement natif | oui | non (le cache HTTP de la webview) |

Fichiers iOS : `src/SonNuage.Mobile/Platforms/iOS/` (`AppDelegate`, `Program`, `Info.plist`, `Entitlements.plist`) et
`Services/PageJsRuntime.cs` (donne l'`IJSRuntime` de la page au lecteur singleton).

## Obtenir l'app sans Mac

1. **Build** : le job `ios` de `.github/workflows/ci.yml` tourne sur un runner macOS de GitHub Actions (gratuit sur un
   dépôt public) et publie l'artefact `sonnuage-ios` : `sonnuage-X.Y.Z-unsigned.ipa`. Lancer le workflow à la main
   (*Actions › CI › Run workflow*) ou pousser sur `main`.
   **GitLab** : le job manuel `ios-ipa` de `.gitlab-ci.yml` fait la même chose, mais exige un runner macOS. Sur
   gitlab.com, les runners macOS SaaS sont réservés aux offres Premium/Ultimate ; en offre gratuite, il faut un Mac
   auto-hébergé comme runner. À défaut, GitHub Actions reste la voie sans Mac.
2. **Signature** : l'IPA n'est pas signé. Avec un **Apple ID gratuit**, le signer depuis Linux ou Windows :
   - [Sideloadly](https://sideloadly.io) (Windows/macOS), ou [Sideloader](https://github.com/Dadoum/Sideloader) /
     [AltServer-Linux](https://github.com/NyaMisty/AltServer-Linux) sous Linux ; l'iPhone est branché en USB.
   - Limites du compte gratuit : l'app expire au bout de **7 jours** (à re-signer), 3 apps maximum.
   - Avec un compte développeur payant (99 $/an) : signature 1 an, TestFlight possible.
3. Sur l'iPhone : Réglages › Général › VPN et gestion de l'appareil › faire confiance au profil.

## Risques connus (non vérifiables sans appareil)

- **Trousseau** : `SecureStorage` garde la session dans le Keychain. Si l'outil de signature ne fournit pas
  `keychain-access-groups`, la lecture échoue ; `MobileStore` réinitialise alors le stockage (reconnexion à chaque lancement).
- **Lecture en arrière-plan** : repose sur la webview (WKWebView). Elle fonctionne quand la session audio est active
  et que la lecture a été lancée par un geste de l'utilisateur ; à valider sur appareil, iOS pouvant suspendre l'audio
  web à l'écran verrouillé selon la version.
- **Build** : le workload iOS de .NET vise une version d'iOS précise selon la bande du SDK (10.0.3xx : iOS 26.5 ;
  10.0.4xx : iOS 27.x), qui doit être celle du SDK d'Xcode. La CI épingle donc le SDK 10.0.3xx (`global.json` et
  `setup-dotnet`) et `Xcode_26.5.app`. Sur le runner macOS de GitHub, `Xcode.app`, `Xcode_26.6.0.app` et
  `Xcode_26.5.0.app` sont inutilisables (xcrun échoue). Au changement de bande ou d'image du runner, tester les
  Xcode disponibles en parallèle avec une matrice, ne pas les essayer un par un.
- **`global.json`** impose le SDK 10.0.300 : un autre SDK .NET local ne compile pas ce dépôt.

## Build local (macOS)

```bash
dotnet workload install maui-ios
dotnet build src/SonNuage.Mobile -c Release -p:RuntimeIdentifier=iossimulator-arm64
```

Le projet ne vise que `net10.0-ios` : il ne se compile pas sous Linux ou Windows (Xcode requis).
