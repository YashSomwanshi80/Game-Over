# Contributing to GAME OVER

Every change to GAME OVER ships as a **numbered version**. Nothing is overwritten or deleted: each version keeps its own record in [`versions/`](versions/), its own git tag, and its own APK on the Releases page. Anyone can go back to any earlier version at any time.

## The rules

1. **Every change gets a new version.** Never edit, rename or delete an earlier version's record, tag or release. Never reuse a version number.
2. **Versions are `MAJOR.MINOR.PATCH`** (for example `0.3.28`).
   - **PATCH** (`0.3.28` to `0.3.29`): fixes and small features, such as a new setting or a layout change.
   - **MINOR** (`0.3.x` to `0.4.0`): bigger features, such as a new room, mode or mechanic.
   - **MAJOR** (`0.x` to `1.0.0`): a release you consider finished and stable.
   - The Android version code goes up by **1** every release (`v0.3.28` is code 58).
3. **Newest first.** The version list in `versions/README.md` and the "Latest Version" section of the main README always put the newest version at the top.
4. **Never rewrite published history.** Do not force-push `main`, move tags, or delete releases.
5. **Do not commit APKs.** They go on the GitHub Release only (`.gitignore` already blocks `*.apk`).
6. **Use your own identity.** Commit with your own name and email. Do not add other people's names or co-author lines unless they actually contributed.

## Steps for every contribution

1. **Get the latest code**, and work on a branch (`git checkout -b my-change`). Outside contributors open a pull request; the owner merges it.
2. **Make your change** and test it in Unity (6000.3.21f1 is the project version).
3. **Pick the next version number** from the top of [`versions/README.md`](versions/README.md).
4. **Bump the version in the build script.** In `Assets/Editor/GameOverAndroidBuild.cs`, set `PlayerSettings.bundleVersion` to the new version and raise the number in `Math.Max(<code>, ...)` by 1.
5. **Add the version record.** Copy [`versions/TEMPLATE.md`](versions/TEMPLATE.md) to `versions/vX.Y.Z.md` and fill it in. Do not touch the older files.
6. **Update the indexes.**
   - Add one line for the new version at the **top** of the table in `versions/README.md`.
   - Update the download table and the "Latest Version" section in the main `README.md`. Keep the previous version's link in the table.
7. **Build the Android APK** and record its SHA-256 checksum. For example: `sha256sum GAME-OVER-vX.Y.Z.apk`.
8. **Commit** with the message format `vX.Y.Z: short description`, for example `v0.3.29: fix phone call interruption`.
9. **Merge to `main`, tag and push.**
   ```bash
   git tag vX.Y.Z
   git push origin main --tags
   ```
10. **Create the release** on GitHub for that tag, attach `GAME-OVER-vX.Y.Z.apk`, and paste the changes and checksum into the release notes. Mark it as the latest release. Leave all older releases in place.
11. **Check the result.** Open the new download link, confirm the APK downloads, and confirm the previous version's link still works.

## What "the last version is kept" means

- `versions/vX.Y.Z.md` records what changed, the commit it was made from, the checksum, and the download link.
- The tag `vX.Y.Z` points at the exact source for that version. To get an old version's code: `git checkout v0.3.27`.
- The Release for each version keeps its APK, so old builds stay downloadable.

## Checklist before you push

- [ ] Version bumped in `GameOverAndroidBuild.cs` (name and code)
- [ ] New file `versions/vX.Y.Z.md` added, older files untouched
- [ ] `versions/README.md` has the new line at the top
- [ ] Main `README.md` download table and latest-version section updated
- [ ] APK built and checksum recorded
- [ ] Tag created and pushed, release published with the APK
- [ ] Old tags, releases and version files still exist
