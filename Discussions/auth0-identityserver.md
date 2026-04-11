# Auth0 vs Identity Server

## Problem

Currently the project is plugged in to use Auth0 as our authentication provider. The app hasn't went live yet so we can change our authentication provider if there is a better use case. Auth0 handles all the PII data like names, emails and roles etc. In order to be able to filter on that data we are having to keep a copy of it in our own DB as the auth0 Management API can cost a lot when requests ramp up. We're also concerned that rate limiting may end up kicking in when under load. IdentityServer would keep all this data in our own DB so easier to query and no rate limiting issues but it means we have to manage all the data as well.

## Constraints

- There is none as yet as haven't went live.

## Decision

**Stay on Auth0.**

### Rationale

- Rate limiting and Management API cost concerns are theoretical at current scale — not a real problem to solve now.
- MFA is a planned requirement. Auth0 provides this out of the box; IdentityServer would require building and maintaining it from scratch.
- Solo developer: IdentityServer's operational burden (hosting, patching, key rotation, auth flow maintenance) is a real and ongoing cost that outweighs the benefits.
- Data duplication (local PII copy for querying) is the correct pattern when you don't own the identity provider. It is not a design smell.
- `ExternalProviderId` on `ApplicationUser` decouples the app from Auth0 as a specific provider, preserving a migration path if the decision needs to be revisited at scale.

### Management API integration scope

The following operations require Auth0 Management API calls and must be implemented:

- User creation (triggered when admin adds a user via `POST /api/users`)
- Role assignment on creation
- Role updates when an admin changes a user's role
- User disable/block in Auth0 when `IsActive` is set to false
