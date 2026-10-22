#!/usr/bin/env bash
# Checks a PR against CONTRIBUTING.md. Expects HEAD = base branch and a ref "pr-head" = PR commits.
# DRY_RUN=1 prints the comment instead of posting it (used for local testing).
set -u
BASE=$(git merge-base HEAD pr-head)
fails=()

# 1. No APKs in the repository.
if git diff --name-only --diff-filter=AM "$BASE" pr-head | grep -qi '\.apk$'; then
  fails+=("An APK file was committed. APKs go on the GitHub Release only, never in the repo.")
fi

# 2. Earlier version records must never be edited, renamed or deleted.
bad=$(git diff --name-status "$BASE" pr-head | awk '$1 ~ /^(M|D|R)/' | grep -E 'versions/v[0-9]' || true)
if [ -n "$bad" ]; then
  fails+=("An earlier version record in versions/ was changed or removed. Old versions must stay exactly as they are.")
fi

# 3. Game changes need a new version number, a version record and an index entry.
code=$(git diff --name-only "$BASE" pr-head | grep -E '^(Assets|Packages|ProjectSettings)/' | grep -v '\.meta$' || true)
getv() { git show "$1:Assets/Editor/GameOverAndroidBuild.cs" 2>/dev/null | grep -oE 'bundleVersion="[0-9]+\.[0-9]+\.[0-9]+' | grep -oE '[0-9]+\.[0-9]+\.[0-9]+' | head -1; }
oldv=$(getv "$BASE"); newv=$(getv pr-head)
if [ -n "$code" ]; then
  if [ -z "$newv" ] || [ "$newv" = "$oldv" ] || [ "$(printf '%s\n%s\n' "$oldv" "$newv" | sort -V | tail -1)" != "$newv" ]; then
    fails+=("The game was changed but the version was not raised. Bump bundleVersion (currently ${oldv:-unknown}) and the version code in Assets/Editor/GameOverAndroidBuild.cs.")
  else
    git cat-file -e "$BASE:versions/v$newv.md" 2>/dev/null && fails+=("Version $newv already exists. Pick the next unused version number.")
    git cat-file -e "pr-head:versions/v$newv.md" 2>/dev/null || fails+=("Missing versions/v$newv.md. Copy versions/TEMPLATE.md and fill it in.")
    git show pr-head:versions/README.md 2>/dev/null | grep -q "v$newv" || fails+=("versions/README.md has no line for v$newv. Add it at the top of the table.")
  fi
fi

bumped=""; [ -n "$code" ] && [ -n "$newv" ] && [ "$newv" != "$oldv" ] && bumped=1
marker='<!-- pr-rules-check -->'
if [ ${#fails[@]} -eq 0 ]; then
  body="$marker
**Rules check: passed.** This PR follows the CONTRIBUTING.md rules that can be checked automatically.${bumped:+ New version: v$newv.}

_This only checks the contributing rules. It does not review the game code._"
  status=0
else
  list=""; for f in "${fails[@]}"; do list="$list- $f"$'\n'; done
  body="$marker
**Rules check: needs changes.** Please fix the following (see CONTRIBUTING.md):

$list
_This only checks the contributing rules. It does not review the game code._"
  status=1
fi

if [ "${DRY_RUN:-0}" = "1" ]; then echo "$body"; exit $status; fi
existing=$(gh api "repos/$REPO/issues/$PR_NUMBER/comments" --jq ".[] | select(.body|contains(\"$marker\")) | .id" | head -1)
if [ -n "$existing" ]; then gh api -X PATCH "repos/$REPO/issues/comments/$existing" -f body="$body" >/dev/null
else gh api "repos/$REPO/issues/$PR_NUMBER/comments" -f body="$body" >/dev/null; fi
exit $status
