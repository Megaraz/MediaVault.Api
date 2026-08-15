# User and media write validation policy

MediaVault validates user and media write DTOs in the Application layer before availability checks, mapping, or repository writes. The policy is shared by `UserDtoValidator` and `MediaEntryDtoValidator`; it is not a replacement for database integrity.

## Input bounds

- Usernames: 50 characters.
- Emails: 254 characters and a valid email address.
- Passwords: 128 characters.
- Media titles: 200 characters.
- External identifiers: 100 characters.
- Reviews and overviews: 4,000 characters each.
- Authors and airing status: 200 and 100 characters respectively.
- HTTP/HTTPS URL fields: 2,048 characters and absolute URLs only.
- Genres and platforms: at most 20 entries, with at most 50 characters per entry.
- TV series seasons: at most 100 nested entries.
- PC requirement text: at most 2,000 characters per field.
- Ratings: 0 through 5 in 0.5 increments.
- Status values must be defined `Status` enum values.
- Numeric counters reject negative values and use product-safe upper bounds for runtime, play time, scores, seasons, and episodes.

Optional text values may be omitted. Supplied text cannot contain control characters. Nested season and PC requirement fields use the same policy and report their field path in the validation response.

## User identifiers

Registration, profile updates, and login canonicalize identifiers before validation and persistence. The canonical form trims surrounding whitespace and lowercases both usernames and email addresses. Username casing is therefore explicitly case-insensitive, and clients may continue sending mixed-case or padded values. Responses and JWT claims use the canonical stored values.

## HTTP limits and contracts

Registration, profile updates, and media create/update actions have a 256 KiB request-body ceiling through `RequestSizeLimit`. A request over that limit is rejected with `413 Payload Too Large`. Invalid DTO values continue to use the existing `422 Unprocessable Entity` response and `ValidationErrorResponseBody` contract.

Authenticated profile and media state-changing actions use the `AuthenticatedWriteByUser` fixed-window limiter: 30 permits per validated user per 60 seconds, with no queue. The policy covers profile update, media create/update, and media delete. Login and registration remain governed by their existing IP policies, and provider metadata remains governed by its existing per-user policies.

The limiter partitions on the validated JWT user identifier. It does not trust a client-supplied owner identifier or forwarded IP header. Rejected requests retain the existing `429` JSON body and `Retry-After` behavior.

The domain `Rating` value object may still normalize values when materializing an entity or reading persisted data. Incoming write DTOs must pass the explicit Application-layer range and step validation before mapping.
