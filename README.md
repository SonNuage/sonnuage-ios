# SonNuage iOS

**English** · [Français](README.fr.md)

iOS app for [SonNuage](https://github.com/SonNuage/sonnuage), a self-hosted music server: .NET MAUI
Blazor Hybrid (same pages as the web client), target `net10.0-ios`. Playback goes through the shared web player
(`WebPlayer` from `SonNuage.UI`), like the desktop app. This repository continues the history of
[sonnuage-android](https://github.com/SonNuage/sonnuage-android), which remains the reference for the Android app.
iOS details, installing without a Mac and known risks: [docs/IOS.md](docs/IOS.md) (in French).

Status: alpha testing, unsigned build.

| Folder | Role |
|---|---|
| `src/SonNuage.Mobile` | App: Blazor host, iOS audio session, secure storage of the accounts, crash log |
| `docs/` | iOS notes (build, signing, limits) |

The pages and components come from the `SonNuage.UI` library of the
[sonnuage](https://github.com/SonNuage/sonnuage) repository:

- **in GitHub CI**: sources of the `sonnuage` repository, checked out next to this one
- **in GitLab CI**: NuGet package published by the CI of `sonnuage` (version in `Directory.Packages.props`)
- **in development**: if `sonnuage` is cloned next to this repository (`../sonnuage`), its sources are used
  directly (see `Directory.Build.props`, property `SonNuageRepo`).

To follow a new server version, raise `SonNuage.UI` in `Directory.Packages.props` to the published
version (`X.Y.Z` for a server tag, `0.1.0-ci.N` for a build of `main`).

## How it works

- **Accounts**: on first launch, the app asks for the server address, then the login. Several
  accounts (one user on one server, possibly several servers) can be signed in at the same time.
  **Settings › Accounts** shows the active account, and lets you add one, change the active account or sign
  out of a given account. The active account sets the library and playback.
- Each account has its own tokens (SecureStorage, that is the iOS keychain), its queue and its
  playback settings.
- **Background playback**: audio session in the playback category and `UIBackgroundModes=audio`; the lock
  screen controls come from the web player's MediaSession.
- **Crashes**: any unhandled exception (.NET, tasks, Blazor components) is written to the `crashes` folder
  of the app's data. On the next launch, a native screen shows the report and offers to send it to the server
  or to copy it. If startup itself fails, this screen replaces the interface.

## CI

- [.github/workflows/ci.yml](.github/workflows/ci.yml): `ios` job on a macOS runner, unsigned `.app` packaged
  as `sonnuage-X.Y.Z-unsigned.ipa` (artifact `sonnuage-ios`). On a `vX.Y.Z` tag, the IPA is attached to a GitHub
  release (pre-release if the tag has a suffix, `v0.1.0-alpha.1`). Optional secret: `SONNUAGE_REPO_TOKEN` (read
  token) if the `sonnuage` repository is private.
- [.gitlab-ci.yml](.gitlab-ci.yml): manual job `ios-ipa`, which requires a macOS runner (see [docs/IOS.md](docs/IOS.md)).

Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/en/): `fix: …` for a
fix, `feat: …` for a new feature; `docs`, `chore`, `ci`, `refactor`, `test` for what changes nothing
for the user.

## Licence

SonNuage is free software under the [GNU AGPL v3.0 or later](LICENSE) licence: anyone can use it,
study it, modify it and redistribute it. A modified version, including one offered as an online service, must
publish its source code under the same licence.

Copyright © 2026 pmj.bailly and the SonNuage contributors.
