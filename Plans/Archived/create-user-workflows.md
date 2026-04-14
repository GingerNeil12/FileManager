# Create User Workflows

## Feature

We need the ability to add a new User to the system. We now have all the building blocks needed for this IE the Auth0ManagementClient, relevant validator and models. The following rules must apply:

- Request model is valid
- InternalUser can only add ExternalUsers
- InternalAdmin can add either InternalUsers OR ExternalUsers
- If an InternalUser is being created then the DepartmentId should be present
- User is created in Auth0 then added to our DB
- Return the new ApplicationUserId

## Context

This is the first of many workflows around user management. There is a validator, repository and auth0 client created ready for the above implementation.

## Expected Outcomes

- Workflow above created with the rules
- Unit tests
  - Passing unit tests as well
- Logging
- Exception handling
- Use of the ICurrentUser interface to access the relevant role of the current user

## Constraints

- No need to check if the user is an ExternalUser as the UserManagementController is checking if the logged in user has the ability to enter this code path
- If more than the CreateUserService class is needed stop and ask for input
- Not completed till linting passes as well

---

## Implementation Notes

### Deviations from Original Plan

| # | Topic | Original | Actual |
|---|-------|----------|--------|
| 1 | InternalAdmin can create InternalAdmin | Not stated | Confirmed yes — no DepartmentId required |
| 2 | Role permission check ordering | After validator | Moved **before** async validator call (avoids wasted I/O on forbidden requests) |
| 3 | Controller invalid-role 400 response | Inline `ValidationProblemDetails` | Refactored to `GetResultFromError(new ValidationError(...))` to reuse existing base class logic |

### Additional Files Created

- `Backend/src/FileManager.WebApi/DTOs/Users/CreateUserDtoExtensions.cs` — DTO→Request mapping (confirmed in scope during planning)
- `Backend/tests/FileManager.Application.Tests/Workflows/CreateUser/CreateUserServiceTests.cs` — 19 unit tests

---

## Files Summary

| File | Action |
|------|--------|
| `Backend/src/FileManager.Application/Workflows/CreateUser/CreateUserService.cs` | Modified — implemented |
| `Backend/src/FileManager.WebApi/Controllers/UsersController.cs` | Modified — `CreateAsync` implemented |
| `Backend/src/FileManager.WebApi/DTOs/Users/CreateUserDtoExtensions.cs` | Created |
| `Backend/tests/FileManager.Application.Tests/Workflows/CreateUser/CreateUserServiceTests.cs` | Created — 19 tests |
