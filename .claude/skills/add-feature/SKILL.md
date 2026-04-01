---
name: add-feature
description: Steps to follow when adding a new feature to the codebase.
---

When adding a new feature we need a plan. The plan will be in the Plans directory in the root of the project. Prompt the user to select whcih plan they want to execure. Eneter plan mode, read the selected plan, use the `grill-me` skill to get all the information needed. Once everything has been confirmed:

1. **Branch** - ensure you are on `develop`, run `git pull`, then create and checkout a new branch following the pattern `feature/<plan-name>`. Perform all work on this branch.
2. **Implement** - build out the feature as per the plan.
3. **Migrations** - If the feature touches Domain or Persistence run `add-migration` to generate the EF Core migration.
4. **Tests** - write tests using `test-writer-dotnet` to generate tests for the .net codebase and `test-writer-angular` for the Angular codebase.
5. **Build and Test** - run builds and test suites to ensure everything is working as expected.
6. **Simplify** - run 'simplify' to refactor and clean up the codebase.
7. **Update the plan** - Update plan.md to reflect any deviations or decisions made during implementation that differ from the original design. Add a final summary table of files created, modified, and deleted.
8. **Archive plan** - once the feature is complete, move the plan from `Plans` to `Plans/Archived`.