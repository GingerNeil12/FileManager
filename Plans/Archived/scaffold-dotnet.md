# Scaffold dotnet project

We are looking to scaffold a new .NET10 project into the Backend directory. This will only create the solution and projects and contain no actual workflow code. We are looking to follow a clean architecture approach.

## Requirements

- 4 projects: WebApi, Application, Persistence and Domain.
- 4 Test projects matching the above but with the naming `<ProjectName>.Tests`.
- The test projects using NUnit as the testing framework, NSubstitute as the mocking framework and the WebApi and Persistence projects having TestContainers nuget package as well. WebApi.Tests will have the nuget package that will allow for use of the WebApplicationFactory for integration tests.
- No tests to be writen or classes created.
- Dockerfile added to the WebApi project that is using an alpine base image.
- Domain has no dependencies.
- Application depends on Domain.
- Persistence and WebApi depend on Application.
- All the testing project depend on their matching implementation project (eg Domain.Tests depends on Domain).
