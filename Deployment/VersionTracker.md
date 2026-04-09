# Version Tracker

## release/1.3.0 - 2026-04-10

### Backend (1.3.0)

- Added base domain models and repository interfaces
- Added DbContext with EF Core configurations and BlobContainerClient DI registration
- Implemented FileMetadata repository with database integration
- Implemented Blob repository with Azure Blob Storage integration
- Updated GlobalExceptionHandler with additional test coverage

### Frontend (1.3.0)

- Added navpanel with submenu support
- Updated version page

---

## release/1.2.0 - 2026-04-07

### Backend (1.2.0)

- Added Auth0 integration with Docker support
- Added current user support for accessing logged-in user info

### Frontend (1.2.0)

- Added default navbar with Auth0 login/logout dropdown
- Integrated Auth0 authentication
- Added role guard and auth-error interceptor
- Updated icons to use ng-icon
- Added default landing page

---

## release/1.1.0 - 2026-04-05

### Backend (1.1.0)

- Added CORS configuration

### Frontend (1.1.0)

- Scaffolded Angular frontend with Docker and build pipeline support
- Added versioning display to Angular app with updated card layout and colouring
- Loaded backend version via API integration

---

## release/1.0.0 - 2026-04-04

### Backend (1.0.0)

- Initial .NET 10 WebAPI scaffold with Result pattern and GlobalExceptionHandler
- Added Version endpoint with Scalar/OpenAPI support
- Added editorconfig and format-on-save hook
- Added VersionController tests
- Added backend CI build pipeline

---
