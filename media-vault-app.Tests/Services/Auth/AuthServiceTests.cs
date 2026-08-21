using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Services.Auth;
using media_vault_app.Application.Validators.User;
using media_vault_app.Tests.TestHelpers;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Errors;
using UserEntity = media_vault_app.Domain.Entities.User;

namespace media_vault_app.Tests.Services.Auth
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LoginAsync_Should_ReturnValidationFailure_When_LoginDtoIsInvalid()
        {
            var userRepo = new FakeUserRepo();
            var passwordHasher = new FakePasswordHasherService();
            var service = CreateService(userRepo, passwordHasher);

            var result = await service.LoginAsync(new UserLoginDto("", ""));

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(2, result.ValidationErrors.Count);
            Assert.Equal(0, userRepo.GetByUsernameOrEmailCallCount);
            Assert.Equal(0, passwordHasher.VerifyPasswordCallCount);
        }

        [Fact]
        public async Task LoginAsync_Should_Propagate_RepoFailure_When_UserLookupFails()
        {
            var expectedError = MediaVaultErrors.NotFound(DefineErrorContext(nameof(AuthService.LoginAsync), OperationType.Get));
            var userRepo = new FakeUserRepo
            {
                GetByUsernameOrEmailResult = Result<UserEntity>.Failure(expectedError, "User not found.")
            };

            var service = CreateService(userRepo, new FakePasswordHasherService());

            var result = await service.LoginAsync(new UserLoginDto("missing@example.com", "Password123"));

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal("User not found.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_Should_ReturnUnauthorized_When_PasswordDoesNotMatch()
        {
            var user = CreateUser();
            var userRepo = new FakeUserRepo
            {
                GetByUsernameOrEmailResult = Result<UserEntity>.Success(user)
            };

            var passwordHasher = new FakePasswordHasherService
            {
                VerifyPasswordResult = false
            };

            var service = CreateService(userRepo, passwordHasher);

            var result = await service.LoginAsync(new UserLoginDto(user.Email, "wrong-password"));

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Unauthorized, result.PrimaryError.Type);
            Assert.Equal("wrong-password", passwordHasher.LastProvidedPassword);
            Assert.Equal(user.PasswordHash, passwordHasher.LastHashedPassword);
        }

        [Fact]
        public async Task LoginAsync_Should_ReturnMappedUser_When_CredentialsAreValid()
        {
            var user = CreateUser();
            var userRepo = new FakeUserRepo
            {
                GetByUsernameOrEmailResult = Result<UserEntity>.Success(user)
            };

            var passwordHasher = new FakePasswordHasherService
            {
                VerifyPasswordResult = true
            };

            var service = CreateService(userRepo, passwordHasher);

            var result = await service.LoginAsync(new UserLoginDto(user.Username, "Password123"));

            Assert.True(result.IsSuccess);
            Assert.Equal(user.Id, result.Value.Id);
            Assert.Equal(user.Username, result.Value.Username);
            Assert.Equal(user.Email, result.Value.Email);
        }

        [Fact]
        public async Task LoginAsync_Should_CanonicalizeIdentifierBeforeLookup()
        {
            var user = CreateUser();
            user.Username = " TestUser ";
            user.Email = " TEST@Example.COM ";
            var userRepo = new FakeUserRepo
            {
                GetByUsernameOrEmailResult = Result<UserEntity>.Success(user)
            };

            var service = CreateService(userRepo, new FakePasswordHasherService());

            var result = await service.LoginAsync(
                new UserLoginDto(" TEST@EXAMPLE.COM ", "Password123"));

            Assert.True(result.IsSuccess);
            Assert.Equal("test@example.com", userRepo.RequestedUsernameOrEmail);
            Assert.Equal("testuser", result.Value.Username);
            Assert.Equal("test@example.com", result.Value.Email);
        }

        [Fact]
        public async Task RegisterUserAsync_Should_ReturnValidationFailure_When_RegisterDtoIsInvalid()
        {
            var userRepo = new FakeUserRepo();
            var passwordHasher = new FakePasswordHasherService();
            var service = CreateService(userRepo, passwordHasher);

            var result = await service.RegisterUserAsync(new UserRegisterDto("", "email@example.com", "email@example.com", "Password123", "Password123"));

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(0, userRepo.AvailabilityCallCount);
            Assert.Equal(0, passwordHasher.HashPasswordCallCount);
        }

        [Fact]
        public async Task RegisterUserAsync_Should_Propagate_NonValidationAvailabilityFailure()
        {
            var expectedError = MediaVaultErrors.Failure(DefineErrorContext(nameof(AuthService.RegisterUserAsync), OperationType.Create), "Availability check failed.");
            var userRepo = new FakeUserRepo
            {
                AvailabilityResult = Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Failure(expectedError, "Availability check failed.")
            };

            var service = CreateService(userRepo, new FakePasswordHasherService());

            var result = await service.RegisterUserAsync(CreateRegisterDto());

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal("Availability check failed.", result.Message);
        }

        [Fact]
        public async Task RegisterUserAsync_Should_ReturnValidationFailure_When_UsernameAndEmailAreUnavailable()
        {
            var userRepo = new FakeUserRepo
            {
                AvailabilityResult = Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success((false, false))
            };

            var service = CreateService(userRepo, new FakePasswordHasherService());

            var result = await service.RegisterUserAsync(CreateRegisterDto());

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.PrimaryError.Type);
            Assert.Equal(2, result.ValidationErrors.Count);
            Assert.Equal("The username and email are already registered.", result.Message);
            Assert.Equal(0, userRepo.RegisterUserCallCount);
        }

        [Fact]
        public async Task RegisterUserAsync_Should_HashPassword_And_RegisterUser_When_RequestIsValid()
        {
            var userRepo = new FakeUserRepo
            {
                AvailabilityResult = Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success((true, true)),
                RegisterUserResult = Result.Success()
            };

            var passwordHasher = new FakePasswordHasherService
            {
                HashPasswordResult = "hashed::Password123"
            };

            var service = CreateService(userRepo, passwordHasher);

            var result = await service.RegisterUserAsync(CreateRegisterDto());

            Assert.True(result.IsSuccess);
            Assert.Equal(1, passwordHasher.HashPasswordCallCount);
            Assert.Equal("Password123", passwordHasher.LastPasswordToHash);
            Assert.NotNull(userRepo.RegisteredEntity);
            Assert.Equal("hashed::Password123", userRepo.RegisteredEntity!.PasswordHash);
            Assert.Equal("testuser", userRepo.RegisteredEntity.Username);
            Assert.Equal("test@example.com", userRepo.RegisteredEntity.Email);
        }

        [Fact]
        public async Task RegisterUserAsync_Should_CanonicalizeIdentifiersBeforeValidationAndStorage()
        {
            var userRepo = new FakeUserRepo
            {
                AvailabilityResult = Result<(bool IsUserNameAvailable, bool IsEmailAvailable)>.Success((true, true)),
                RegisterUserResult = Result.Success()
            };

            var service = CreateService(userRepo, new FakePasswordHasherService());
            var result = await service.RegisterUserAsync(
                new UserRegisterDto(
                    " TestUser ",
                    " TEST@Example.COM ",
                    " test@example.com ",
                    "Password123",
                    "Password123"));

            Assert.True(result.IsSuccess);
            Assert.Equal(("testuser", "test@example.com"), userRepo.LastAvailabilityRequest);
            Assert.NotNull(userRepo.RegisteredEntity);
            Assert.Equal("testuser", userRepo.RegisteredEntity!.Username);
            Assert.Equal("test@example.com", userRepo.RegisteredEntity.Email);
        }

        [Fact]
        public async Task RegisterUserAsync_Should_Propagate_RegisterFailure()
        {
            var expectedError = MediaVaultErrors.Conflict(DefineErrorContext(nameof(AuthService.RegisterUserAsync), OperationType.Create));
            var userRepo = new FakeUserRepo
            {
                RegisterUserResult = Result.Failure(expectedError, "Could not create user.")
            };

            var service = CreateService(userRepo, new FakePasswordHasherService());

            var result = await service.RegisterUserAsync(CreateRegisterDto());

            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.PrimaryError);
            Assert.Equal("Could not create user.", result.Message);
        }

        private static AuthService CreateService(FakeUserRepo userRepo, FakePasswordHasherService passwordHasher)
        {
            return new AuthService(
                userRepo,
                passwordHasher,
                new UserDtoValidator(),
                ServiceTestLogger.Create<AuthService>());
        }

        private static UserEntity CreateUser()
        {
            return new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "stored-hash",
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
        }

        private static UserRegisterDto CreateRegisterDto()
        {
            return new UserRegisterDto(
                Username: "testuser",
                Email: "test@example.com",
                ConfirmEmail: "test@example.com",
                Password: "Password123",
                ConfirmPassword: "Password123");
        }

        private static ErrorContext DefineErrorContext(string methodName, OperationType operation)
        {
            return new ErrorContext(
                operation: operation,
                entityName: "User");
        }
    }
}
