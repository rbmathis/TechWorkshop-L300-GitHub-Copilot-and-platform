#!/usr/bin/env bash
set -euo pipefail

FILE="src/ZavaStorefront.csproj"
BUMP_KIND="${1:-auto}"
BASE_REF="${2:-}"

if [[ ! -f "$FILE" ]]; then
  echo "File not found: $FILE" >&2
  exit 1
fi

current_version="$(grep -oPm1 '(?<=<Version>)[^<]+' "$FILE" || true)"
if [[ -z "$current_version" ]]; then
  current_version="1.0.0"
fi

# Auto-detect bump type from commit messages if BUMP_KIND is "auto"
if [[ "$BUMP_KIND" == "auto" ]]; then
  echo "Auto-detecting version bump from commit messages..."
  
  # Get commit messages since base ref (or last tag)
  if [[ -n "$BASE_REF" ]]; then
    commits=$(git log "${BASE_REF}..HEAD" --pretty=format:"%s" 2>/dev/null || echo "")
  else
    # Get commits since the last tag, or all commits if no tags exist
    last_tag=$(git describe --tags --abbrev=0 2>/dev/null || echo "")
    if [[ -n "$last_tag" ]]; then
      commits=$(git log "${last_tag}..HEAD" --pretty=format:"%s" 2>/dev/null || echo "")
    else
      commits=$(git log --pretty=format:"%s" 2>/dev/null || echo "")
    fi
  fi
  
  # Analyze commits following Conventional Commits specification
  # BREAKING CHANGE or ! in type/scope = major
  # feat: = minor
  # fix:, chore:, docs:, etc. = patch
  
  BUMP_KIND="patch"  # default
  
  while IFS= read -r commit; do
    # Check for BREAKING CHANGE in commit message
    if echo "$commit" | grep -qiE "BREAKING[- ]CHANGE:|^[a-z]+(\([^)]+\))?!:"; then
      BUMP_KIND="major"
      echo "  - BREAKING CHANGE detected: $commit"
      break
    fi
    # Check for feat: (new feature)
    if echo "$commit" | grep -qiE "^feat(\([^)]+\))?:"; then
      if [[ "$BUMP_KIND" != "major" ]]; then
        BUMP_KIND="minor"
        echo "  - Feature detected: $commit"
      fi
    fi
    # Other types (fix, chore, docs, etc.) stay as patch
    if echo "$commit" | grep -qiE "^(fix|chore|docs|style|refactor|perf|test|build|ci|revert)(\([^)]+\))?:"; then
      echo "  - Non-breaking change: $commit"
    fi
  done <<< "$commits"
  
  echo "Determined bump type: $BUMP_KIND"
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
  echo "bump_type=${BUMP_KIND}" >> "$GITHUB_OUTPUT"
fi
