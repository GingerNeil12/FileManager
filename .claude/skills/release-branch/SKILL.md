---
name: release-branch
description: Steps to follow when creating a release branch for merging into main
---


Ensure both the `main` and `develop` branches are up to date before proceeding. Pull them both from origin.
Follow these steps in order when the user invokes the release-branch skill:

## 1. Detect the last release branch

Run `git branch -a` to find all branches. Identify the release branch with the highest semantic version number (eg `release/2.0.0` takes precedence over `release/1.5.0`). This is the baseline for the new release branch version and for comparing the changes.

If no prior release branch exists, use `develop` as the diff base and start versioning at `1.0.0`.

## 2. Detect which projects have changed

run `git diff release/<last-version>...develop -- Backend/` and `git diff release/<last-version>...develop -- Frontend/` to determine which projects have committed changes since the last release.

Track independently:
- `backendChanged` - true if Backend has any diff output
- `frontendChanged` - true if Frontend has any diff output

If neither has changes, inform the user and stop.

## 3. Prompt user for the version type

Ask: **Is this a major or minor release?**
- **Major** - breaking changes (e.g API contract changes)
- **Minor** - new features, non-breaking changes, UI changes

Do not offer hotifx as an option here. Hotfixes follow a separate process.

## 4. Bump component versions

Only update versions for projects that have changed.

**Backend** (if `backendChanged`):
- File: `Backend/FileManager.WebApi/appsettings.json`
- Key: `ApplicationInfo:Version`
- Read the current value from the file, then increment the major or minor part. Reset the lower parts to 0 (e.g minor bump `1.2.3` -> `1.3.0`, major bump `1.2.3` -> `2.0.0`)

**Frontend** (if `frontendChanged`):
- File: `Frontend/src/environments/environment.ts` and `Frontend/src/environments/environment.development.ts`
- Key: `appVersion`
- Read the current value from each file, then apply the same increment rules. Ensure both files are updated to the same value.

## 5. Create the release branch

Ensure you are on the `develop` branch. What is ready to release will already be merged into it. Determine the new release branch version by applying the same major/minor bump to the last release branch version (eg if last release branch is `release/1.2.3` and this is a minor release, new branch will be `release/1.3.0`).

Run:
```
git checkout develop
git checkout -b release/<new-version>
```

## 6. Update the VersionTracker.md

File: `Deployment/VersionTracker.md`

Prepend a new entry (most recent at top) using the commit history since the last release branch:

```
git log release/<last-version>..develop --oneline
```

Format the entry as follows, including only the sections for the projects that have changed:

```markdown
## release/<new-version> - YYYY-MM-DD

### Backend (<new backend version>)

- <change derived from commit history>
- <change derived from commit history>

### Frontend (<new frontend version>)

- <change derived from commit history>
- <change derived from commit history>

---
```

Prepend the new entry directly below the `# Version Tracker` heading. If the heading does not exist, add it at the top of the file before the first entry. The heading must always remain the first line of the file.

Stage and commit:
```
git add Deployment/VersionTracker.md
git commit -m "docs: update VersionTracker for release/<new-version>"
```