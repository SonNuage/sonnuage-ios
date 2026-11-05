#!/bin/sh
# Installe le workload MAUI iOS dont la version d'iOS est celle du SDK iPhoneOS d'Xcode (le workload le plus récent
# vise souvent un iOS plus récent que l'Xcode du runner : erreurs MT4162). On parcourt les jeux de workloads du plus
# récent au plus ancien jusqu'à trouver le pack Microsoft.iOS.Sdk de la bonne version.
set -eu

sdk=$(xcodebuild -showsdks | grep -o 'iphoneos[0-9][0-9.]*' | head -1 | sed 's/iphoneos//')
echo "SDK iPhoneOS d'Xcode : $sdk"
root="${DOTNET_ROOT:-$HOME/.dotnet}"

dotnet workload search version --take 30 | tee /tmp/workload-versions.txt
versions=$(grep -Eo '^[0-9]+\.[0-9]+\.[0-9]+(\.[0-9]+)?' /tmp/workload-versions.txt || true)
[ -n "$versions" ] || { echo "Aucune version de jeu de workloads listée" >&2; exit 1; }

for v in $versions; do
  echo "== jeu de workloads $v"
  dotnet workload install maui-ios --version "$v" > /tmp/workload-install.log 2>&1 || { tail -5 /tmp/workload-install.log; continue; }
  if ls "$root/packs" 2>/dev/null | grep -q "^Microsoft.iOS.Sdk.*_${sdk}$"; then
    echo "OK : $v fournit le pack iOS $sdk"
    exit 0
  fi
  ls "$root/packs" | grep "^Microsoft.iOS.Sdk" || true
done
echo "Aucun jeu de workloads ne fournit iOS $sdk" >&2
exit 1
