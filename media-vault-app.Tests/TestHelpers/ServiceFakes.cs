using media_vault_app.Application.Interfaces.Repos;
using media_vault_app.Application.Interfaces.Services;
using media_vault_app.Domain.Entities;
using Megaraz.ResultPattern;

namespace media_vault_app.Tests.TestHelpers
{
    using media_vault_app.Application.DTOs.MediaEntry.Response;
    internal sealed class FakeUserRepo : IUserRepo
    {
        public Result<User> GetByIdResult { get; set; } = Result<User>.Success(new User());

        public Result DeleteAccountResult { get; set; } = Result.Success();

        public Result<bool> ExistsResult { get; set; } = Result<bool>.Success(true);

        public Result RegisterUserResult { get; set; } = Result.Success();

        public Result<(bool IsUserNameAvailable, bool IsEmailAvailable)> AvailabilityResult { get; set; } = Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success((true, true));

        public Result<(bool IsUserNameAvailable, bool IsEmailAvailable)> ProfileAvailabilityResult { get; set; } = Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success((true, true));

        public Result ProfileUpdateResult { get; set; } = Result.Success();

        public Result<User> GetByUsernameOrEmailResult { get; set; } = Result<User>.Success(new User());

        public int GetByIdCallCount { get; private set; }

        public int DeleteAccountCallCount { get; private set; }

        public int ExistsCallCount { get; private set; }

        public int RegisterUserCallCount { get; private set; }

        public int AvailabilityCallCount { get; private set; }

        public int ProfileAvailabilityCallCount { get; private set; }

        public int ProfileUpdateCallCount { get; private set; }

        public int GetByUsernameOrEmailCallCount { get; private set; }

        public User? RegisteredEntity { get; private set; }

        public Guid? DeletedId { get; private set; }

        public Guid? RequestedExistsId { get; private set; }

        public Guid? RequestedGetByIdId { get; private set; }

        public string? RequestedUsernameOrEmail { get; private set; }

        public (string Username, string Email)? LastAvailabilityRequest { get; private set; }

        public (Guid UserId, string Username, string Email)? LastProfileUpdateAvailabilityRequest { get; private set; }

        public (Guid UserId, string Username, string Email, int ExpectedVersion)? LastProfileUpdateRequest { get; private set; }

        public Task<Result<User>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            GetByIdCallCount++;
            RequestedGetByIdId = id;
            return Task.FromResult(GetByIdResult);
        }

        public Task<Result> DeleteAccountAsync(Guid id, CancellationToken ct = default)
        {
            DeleteAccountCallCount++;
            DeletedId = id;
            return Task.FromResult(DeleteAccountResult);
        }

        public Task<Result<bool>> ExistsAsync(Guid id, CancellationToken ct = default)
        {
            ExistsCallCount++;
            RequestedExistsId = id;
            return Task.FromResult(ExistsResult);
        }

        public Task<Result> RegisterUserAsync(User entity, CancellationToken ct = default)
        {
            RegisterUserCallCount++;
            RegisteredEntity = entity;
            return Task.FromResult(RegisterUserResult);
        }

        public Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckRegistrationAvailabilityAsync(string username, string email, CancellationToken ct = default)
        {
            AvailabilityCallCount++;
            LastAvailabilityRequest = (username, email);
            return Task.FromResult(AvailabilityResult);
        }

        public Task<Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>> CheckProfileUpdateAvailabilityAsync(Guid userId, string username, string email, CancellationToken ct = default)
        {
            ProfileAvailabilityCallCount++;
            LastProfileUpdateAvailabilityRequest = (userId, username, email);
            return Task.FromResult(ProfileAvailabilityResult);
        }

        public Task<Result> UpdateProfileAsync(
            Guid userId,
            string username,
            string email,
            int expectedVersion,
            CancellationToken ct = default)
        {
            ProfileUpdateCallCount++;
            LastProfileUpdateRequest = (userId, username, email, expectedVersion);
            return Task.FromResult(ProfileUpdateResult);
        }

        public Task<Result<User>> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
        {
            GetByUsernameOrEmailCallCount++;
            RequestedUsernameOrEmail = usernameOrEmail;
            return Task.FromResult(GetByUsernameOrEmailResult);
        }
    }

    internal sealed class FakeMediaEntryRepo : IMediaEntryRepo
    {
        public Result<MediaEntry> CreateResult { get; set; } = Result<MediaEntry>.Success(new MovieEntry());

        public Result<MediaEntry> GetByIdResult { get; set; } = Result<MediaEntry>.Success(new MovieEntry());

        public Result<IReadOnlyList<MediaEntryMinimalDto>> MinimalCollectionByOwnerIdResult { get; set; } = Result<IReadOnlyList<MediaEntryMinimalDto>>.Success(Array.Empty<MediaEntryMinimalDto>());

        public Result<IReadOnlyList<MediaEntryMinimalDto>> SearchMediaEntriesResult { get; set; } = Result<IReadOnlyList<MediaEntryMinimalDto>>.Success(Array.Empty<MediaEntryMinimalDto>());

        public Result UpdateResult { get; set; } = Result.Success();

        public Result DeleteResult { get; set; } = Result.Success();

        public int CreateCallCount { get; private set; }

        public int GetByIdCallCount { get; private set; }

        public int GetMinimalCollectionByOwnerIdCallCount { get; private set; }

        public int SearchCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public int DeleteCallCount { get; private set; }

        public MediaEntry? CreatedEntity { get; private set; }

        public MediaEntry? UpdatedEntity { get; private set; }

        public Guid? LastOwnerId { get; private set; }

        public Guid? LastDependentId { get; private set; }

        public int? LastExpectedVersion { get; private set; }

        public (Guid OwnerId, int PageNumber, int PageSize)? LastCollectionRequest { get; private set; }

        public (Guid OwnerId, string Query, int PageNumber, int PageSize)? LastSearchRequest { get; private set; }

        public Task<Result<MediaEntry>> CreateAsync(MediaEntry entity, CancellationToken ct = default)
        {
            CreateCallCount++;
            CreatedEntity = entity;
            return Task.FromResult(CreateResult);
        }

        public Task<Result> DeleteAsync(
            Guid ownerId,
            Guid dependentEntityId,
            int expectedVersion,
            CancellationToken ct = default)
        {
            DeleteCallCount++;
            LastOwnerId = ownerId;
            LastDependentId = dependentEntityId;
            LastExpectedVersion = expectedVersion;
            return Task.FromResult(DeleteResult);
        }

        public Task<Result<MediaEntry>> GetDetailedByIdAsync(Guid ownerId, Guid entityId, CancellationToken ct = default)
        {
            GetByIdCallCount++;
            LastOwnerId = ownerId;
            LastDependentId = entityId;
            return Task.FromResult(GetByIdResult);
        }

        public Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> GetMinimalCollectionByOwnerIdAsync(Guid ownerId, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            GetMinimalCollectionByOwnerIdCallCount++;
            LastCollectionRequest = (ownerId, pageNumber, pageSize);
            return Task.FromResult(MinimalCollectionByOwnerIdResult);
        }

        public Task<Result> UpdateMovieAsync(Guid ownerId, MovieEntry entity, CancellationToken ct = default) =>
            RecordUpdate(ownerId, entity);

        public Task<Result> UpdateTvSeriesAsync(Guid ownerId, TvSeriesEntry entity, CancellationToken ct = default) =>
            RecordUpdate(ownerId, entity);

        public Task<Result> UpdateGameAsync(Guid ownerId, GameEntry entity, CancellationToken ct = default) =>
            RecordUpdate(ownerId, entity);

        public Task<Result> UpdateBookAsync(Guid ownerId, BookEntry entity, CancellationToken ct = default) =>
            RecordUpdate(ownerId, entity);

        public Task<Result> UpdateMangaAsync(Guid ownerId, MangaEntry entity, CancellationToken ct = default) =>
            RecordUpdate(ownerId, entity);

        private Task<Result> RecordUpdate(Guid ownerId, MediaEntry updatedDependentEntity)
        {
            UpdateCallCount++;
            LastOwnerId = ownerId;
            UpdatedEntity = updatedDependentEntity;
            LastExpectedVersion = updatedDependentEntity.Version;
            return Task.FromResult(UpdateResult);
        }

        public Task<Result<IReadOnlyList<MediaEntryMinimalDto>>> SearchMediaEntriesAsync(Guid userId, string query, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            SearchCallCount++;
            LastSearchRequest = (userId, query, pageNumber, pageSize);
            return Task.FromResult(SearchMediaEntriesResult);
        }
    }

    internal sealed class FakePasswordHasherService : IPasswordHasherService
    {
        public string HashPasswordResult { get; set; } = "hashed-password";

        public bool VerifyPasswordResult { get; set; } = true;

        public int HashPasswordCallCount { get; private set; }

        public int VerifyPasswordCallCount { get; private set; }

        public string? LastPasswordToHash { get; private set; }

        public string? LastHashedPassword { get; private set; }

        public string? LastProvidedPassword { get; private set; }

        public string HashPassword(string password)
        {
            HashPasswordCallCount++;
            LastPasswordToHash = password;
            return HashPasswordResult;
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            VerifyPasswordCallCount++;
            LastHashedPassword = hashedPassword;
            LastProvidedPassword = providedPassword;
            return VerifyPasswordResult;
        }
    }
}
