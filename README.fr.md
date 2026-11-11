# SonNuage iOS

[English](README.md) · **Français**

Application iOS de [SonNuage](https://github.com/SonNuage/sonnuage), serveur de musique auto-hébergé : .NET MAUI
Blazor Hybrid (mêmes pages que le client web), cible `net10.0-ios`. La lecture passe par le lecteur web partagé
(`WebPlayer` de `SonNuage.UI`), comme l'appli de bureau. Ce dépôt reprend l'historique de
[sonnuage-android](https://github.com/SonNuage/sonnuage-android), qui reste la référence de l'application Android.
Détails iOS, installation sans Mac et risques connus : [docs/IOS.md](docs/IOS.md).

État : en test alpha, build non signé.

| Dossier | Rôle |
|---|---|
| `src/SonNuage.Mobile` | Application : hôte Blazor, session audio iOS, stockage sécurisé des comptes, journal de plantage |
| `docs/` | Notes iOS (build, signature, limites) |

Les pages et composants viennent de la bibliothèque `SonNuage.UI` du dépôt
[sonnuage](https://github.com/SonNuage/sonnuage) :

- **en CI GitHub** : sources du dépôt `sonnuage`, extrait à côté de celui-ci
- **en CI GitLab** : paquet NuGet publié par la CI de `sonnuage` (version dans `Directory.Packages.props`)
- **en développement** : si `sonnuage` est cloné à côté de ce dépôt (`../sonnuage`), ses sources sont utilisées
  directement (voir `Directory.Build.props`, propriété `SonNuageRepo`).

Pour suivre une nouvelle version du serveur, monter `SonNuage.UI` dans `Directory.Packages.props` à la version
publiée (`X.Y.Z` pour un tag du serveur, `0.1.0-ci.N` pour un build de `main`).

## Fonctionnement

- **Comptes** : au premier lancement, l'application demande l'adresse du serveur puis l'identifiant. Plusieurs
  comptes (un utilisateur sur un serveur, éventuellement plusieurs serveurs) peuvent être connectés en même temps.
  **Réglages › Comptes** affiche le compte actif, permet d'en ajouter un, de changer de compte actif ou de se
  déconnecter d'un compte précis. Le compte actif fixe la bibliothèque et la lecture.
- Chaque compte a ses propres jetons (SecureStorage, c'est-à-dire le trousseau iOS), sa file d'attente et ses
  réglages de lecture.
- **Lecture en arrière-plan** : session audio en catégorie playback et `UIBackgroundModes=audio` ; les contrôles de
  l'écran verrouillé viennent de la MediaSession du lecteur web.
- **Plantages** : toute exception non gérée (.NET, tâches, composants Blazor) est écrite dans le dossier `crashes`
  des données de l'appli. Au lancement suivant, un écran natif affiche le rapport, propose de l'envoyer au serveur
  ou de le copier. Si le démarrage lui-même échoue, cet écran remplace l'interface.

## CI

- [.github/workflows/ci.yml](.github/workflows/ci.yml) : job `ios` sur un runner macOS, `.app` non signé empaqueté
  en `sonnuage-X.Y.Z-unsigned.ipa` (artefact `sonnuage-ios`). Sur un tag `vX.Y.Z`, l'IPA est joint à une release
  GitHub (pré-version si le tag a un suffixe, `v0.1.0-alpha.1`). Secret facultatif : `SONNUAGE_REPO_TOKEN` (jeton en
  lecture) si le dépôt `sonnuage` est privé.
- [.gitlab-ci.yml](.gitlab-ci.yml) : job manuel `ios-ipa`, qui exige un runner macOS (voir [docs/IOS.md](docs/IOS.md)).

Les messages de commit suivent [Conventional Commits](https://www.conventionalcommits.org/fr/) : `fix: …` pour un
correctif, `feat: …` pour une nouveauté ; `docs`, `chore`, `ci`, `refactor`, `test` pour ce qui ne change rien
pour l'utilisateur.

## Licence

SonNuage est un logiciel libre sous licence [GNU AGPL v3.0 ou ultérieure](LICENSE) : chacun peut l'utiliser,
l'étudier, le modifier et le redistribuer. Une version modifiée, y compris proposée comme service en ligne, doit
publier son code source sous la même licence.

Copyright © 2026 pmj.bailly et les contributeurs de SonNuage.
