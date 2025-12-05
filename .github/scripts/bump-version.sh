#!/usr/bin/env bash
set -euo pipefail

FILE="src/ZavaStorefront.csproj"
BUMP_KIND="${1:-patch}"

if [[ ! -f "$FILE" ]]; then
  echo "File not found: $FILE" >&2
  exit 1
fi

current_version="$(grep -oPm1 '(?<=<Version>)[^<]+' "$FILE" || true)"
if [[ -z "$current_version" ]]; then
  current_version="1.0.0"
fi

IFS='.' read -r major minor patch <<<"${current_version}"
major=${major:-0}
minor=${minor:-0}
patch=${patch:-0}

case "$BUMP_KIND" in
  major)
    major=$((major + 1)); minor=0; patch=0 ;;
  minor)
    minor=$((minor + 1)); patch=0 ;;
  *)
    patch=$((patch + 1)) ;;
esac

new_version="${major}.${minor}.${patch}"

perl -0pi -e "s|<Version>[^<]+</Version>|<Version>${new_version}</Version>|" "$FILE"

echo "Bumped version: ${current_version} -> ${new_version}"
if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  echo "new_version=${new_version}" >> "$GITHUB_OUTPUT"
fi
