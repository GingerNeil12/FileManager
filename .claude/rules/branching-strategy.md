---
alwaysApply: true
---

# Branching strategy

- Follow `Git Flow` as a general branching strategy
- all new features must be on a new branch
- all hotfixes must be on a new branch
- all feature branches must be branched from `develop`
- all hotfix branches must be branched from `main`
- all feature branches must follow the naming `feature/<plan-name>` (kebab-case)
- all hotfix branches must follow the naming `hotfix/<hotfix-name>` (kebab-case)
- develop must never be committed to directly only through merges
- main must never be committed to directly only through merges
- `develop` will merge to `main` through use of a `release` branch
- all release branches will follow semantic versioning: `release/1.0.0`
- hotfix branches are taken from `main`