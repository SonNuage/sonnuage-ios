#!/bin/sh
# Installe le workload MAUI iOS, puis sélectionne l'Xcode dont le SDK iPhoneOS correspond à la version d'iOS du pack
# Microsoft.iOS.Sdk installé. Le workload d'une bande de SDK .NET vise un iOS précis (10.0.4xx : iOS 27.x) : avec
# un Xcode plus ancien, le lien échoue (MT4162). À lancer depuis le dépôt (global.json fixe la bande du SDK).
set -eu

dotnet --version
dotnet workload install maui-ios
root="${DOTNET_ROOT:-$HOME/.dotnet}"

# Le pack de référence ne vient que du workload installé (le dossier packs garde aussi d'autres versions du pack Sdk).
ios=$(ls "$root/packs" | sed -n 's/^Microsoft\.iOS\.Ref\.net10\.0_//p' | sort -V | tail -1)
[ -n "$ios" ] || { echo "Pack Microsoft.iOS.Ref net10.0 introuvable" >&2; exit 1; }
echo "Version d'iOS du workload : $ios"

log() { echo "$1"; [ -z "${GITHUB_STEP_SUMMARY:-}" ] || echo "- $1" >> "$GITHUB_STEP_SUMMARY"; }

for app in $(ls -d /Applications/Xcode_*.app | sort -V -r); do
  dev="$app/Contents/Developer"
  sdk=$(DEVELOPER_DIR="$dev" xcodebuild -showsdks 2>/dev/null | grep -o 'iphoneos[0-9][0-9.]*' | head -1 | sed 's/iphoneos//')
  if [ "$sdk" != "$ios" ]; then
    log "$(basename "$app") : iPhoneOS ${sdk:-?}, ignoré"
    continue
  fi
  # Certains Xcode du runner sont inutilisables (SDK macOS introuvable : actool échoue à la compilation des icônes).
  if ! DEVELOPER_DIR="$dev" xcrun --find actool > /dev/null 2>&1; then
    log "$(basename "$app") : iPhoneOS $sdk mais actool introuvable, ignoré"
    continue
  fi
  log "$(basename "$app") : iPhoneOS $sdk, retenu"
  sudo xcode-select -s "$app"
  xcodebuild -version
  exit 0
done
echo "Aucun Xcode utilisable du runner ne fournit iPhoneOS $ios" >&2
exit 1
