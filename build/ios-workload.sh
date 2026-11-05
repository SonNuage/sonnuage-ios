#!/bin/sh
# Installe le workload MAUI iOS, puis sélectionne l'Xcode dont le SDK iPhoneOS correspond à la version d'iOS du pack
# Microsoft.iOS.Sdk installé. Le workload d'une bande de SDK .NET vise un iOS précis (10.0.4xx : iOS 27.x) : avec
# un Xcode plus ancien, le lien échoue (MT4162). À lancer depuis le dépôt (global.json fixe la bande du SDK).
set -eu

dotnet --version
dotnet workload install maui-ios
root="${DOTNET_ROOT:-$HOME/.dotnet}"

ios=$(ls "$root/packs" | sed -n 's/^Microsoft\.iOS\.Sdk\.net10\.0_//p' | sort -V | tail -1)
[ -n "$ios" ] || { echo "Pack Microsoft.iOS.Sdk net10.0 introuvable" >&2; exit 1; }
echo "Version d'iOS du workload : $ios"

for app in $(ls -d /Applications/Xcode_*.app | sort -V -r); do
  sdk=$(DEVELOPER_DIR="$app/Contents/Developer" xcodebuild -showsdks | grep -o 'iphoneos[0-9][0-9.]*' | head -1 | sed 's/iphoneos//')
  if [ "$sdk" = "$ios" ]; then
    echo "Xcode retenu : $app (iPhoneOS $sdk)"
    sudo xcode-select -s "$app"
    xcodebuild -version
    exit 0
  fi
done
echo "Aucun Xcode du runner ne fournit iPhoneOS $ios" >&2
exit 1
