# Add backend build pipeline

We need a yml based build pipeline that will be able to be ran on GitHub actions. This build pipeline will build the Backend project and run all tests in the testing projects.

## Requirements

- yml based build pipeline in the Deployment folder
- new pipeline able to run on GitHub actions
- new pipeline builds the Backend dotnet projects
- new pipeline runs all tests in the Backend projects

## Implementation Notes

### Deviations from original plan
- **File location changed**: GitHub Actions only executes workflows from `.github/workflows/`. The pipeline was placed at `.github/workflows/backend-ci.yml` rather than `Deployment/` as originally stated — placing it in `Deployment/` would mean it never runs.

### Decisions made during implementation
- Triggers: push and PR on both `develop` and `main`
- Runner: `ubuntu-latest` (has Docker pre-installed, required for Testcontainers)
- Test results published as GitHub PR annotations via `dorny/test-reporter@v1`
- NuGet packages cached using `actions/setup-dotnet@v4` built-in caching, keyed on `**/*.csproj` hashes
- Repeated values (`DOTNET_VERSION`, `BUILD_CONFIG`, `SOLUTION_PATH`, `TEST_RESULTS_DIR`) extracted to workflow-level `env` block

## File Summary

| Action | File |
|--------|------|
| Created | `.github/workflows/backend-ci.yml` |
