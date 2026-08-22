# SharedKernel boundaries

This document records the post-#171 ownership of the backend abstractions. `Rasmus.SharedKernel` is intentionally small: it is a dependency-neutral home for contracts used by both the domain model and infrastructure policies, not a general application framework.

## Retained in SharedKernel

The following contracts have consumers in more than one backend boundary:

- `IEntity<TKey>` composes identity and server-owned timestamps for domain entities. Infrastructure timestamp and EF policies consume the same contract.
- `IOwnerEntity<TKey>` and `IDependentEntity<TKeyOwner, TKeyDependent>` express ownership and dependent-entity relationships on domain models.
- `ICreatedAtUtc`, `IUpdatedAtUtc`, and `IConcurrencyVersion` are the narrow timestamp and optimistic-concurrency seams used by domain entities and Infrastructure persistence code.
- `MediaVaultErrors` centralizes safe ResultPattern error messages used by both Application workflows and Infrastructure repositories. It contains no database or HTTP dependency.

These types depend only on the published `Megaraz.ResultPattern` core package where required. DTO identity markers and generic CRUD contracts are not part of this boundary.

## Owning-layer policies

- Application owns `MediaVaultValidationError`, identifier/null validation extensions, `PaginationParameters`, and `MediaVaultResultMessages` because they govern Application input and workflow behavior.
- Application validators expose explicit feature contracts such as `IMediaEntryDtoValidator`; they do not inherit a generic SharedKernel validator.
- Infrastructure owns `ExternalServiceResponsePolicy` because it bounds and maps provider HTTP responses at the outbound adapter boundary.
- DTOs are Application models and no longer implement a SharedKernel identity marker. Their identifiers remain ordinary response properties.

## Removed generic framework

After issues #169 and #170 migrated the active user and media callers, #171 removed the unused generic service, repository, mapper, validator, and dependent-entity contracts, together with `ReadServiceBase`, `WriteServiceBase`, and `RepoBase`. Their obsolete tests were removed or retargeted to the concrete repositories and policy owners.

No route, authorization, JSON, Result/ProblemDetails, persistence, cancellation, timestamp, or concurrency behavior is intentionally changed by this ownership cleanup.

## Verification boundary

The application test project owns validation and pagination policy tests. Infrastructure tests own provider-response, repository, timestamp, and concurrency tests. The remaining SharedKernel project contains only the retained contracts and cross-layer error factory.
