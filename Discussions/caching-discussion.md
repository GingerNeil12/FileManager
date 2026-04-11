# Caching discussion

## Problem

Currently the set up with Auth0 is that it will hold all the data like Email, GivenName and FamilyName. These are 3 fields I want to be able to filter on. It also holds the current Active status of the Auth0 account (IE has it been blocked or not), Role that is assigned to the user and the last login date time. These are things I want to be able to see on the User Management screen when it is created.

## Constraints

- Not using the Auth0 Management API for filtering as this adds latency and also can cost a lot as well. Rate limiting will also be an issue with this approach.

## Possible resolutions

- Save all the data from Auth0 in the ApplicationUser model.
  - This opens up our DB to being a target for managing PII data.
- Cache the data locally in some different cache structures that make filtering easier.
  - This could take the form of a few different cache entries.
  - Need to keep the cache in sync which can be difficult and TTL as well.
- Completely shift off of Auth0 and use ID4 instead as that houses the data in my own DB.
