using media_vault_app.Application.DTOs.External_API_Contracts.Rawg;
using media_vault_app.Application.Interfaces.Clients;
using media_vault_app.Application.Services.API;
using media_vault_app.Tests.TestHelpers;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Tests.Services.API
{
    public class RawgApiServiceTests
    {
        [Fact]
        public async Task GetGameByIdAsync_Should_Map_Detailed_Response_To_Dto()
        {
            // Arrange
            var client = new FakeRawgApiClient(
                Result<RawgGameDetailedResponse>.Success(new RawgGameDetailedResponse
                {
                    Id = 42,
                    Slug = "test-game",
                    Name = "Test Game",
                    Description = "Test description",
                    Metacritic = 88,
                    Released = "2024-01-01",
                    BackgroundImage = "https://example.com/image.jpg",
                    Website = "https://example.com",
                    Platforms =
                    [
                        new Platform
                        {
                            Platform1 = new Platform1 { Id = 1, Name = "PC", Slug = "pc" },
                            Requirements = new Requirements
                            {
                                Minimum = "Minimum specs",
                                Recommended = "Recommended specs"
                            }
                        },
                        new Platform
                        {
                            Platform1 = new Platform1 { Id = 2, Name = "PlayStation 5", Slug = "playstation5" }
                        },
                        new Platform
                        {
                            Platform1 = new Platform1 { Id = 3, Name = "PC", Slug = "pc" }
                        }
                    ]
                }));

            var service = new RawgApiService(client, ServiceTestLogger.Create<RawgApiService>());

            // Act
            var result = await service.GetGameByIdAsync(42);

            // Assert
            Assert.True(result.IsSuccess);

            var dto = result.Value;
            Assert.Equal(42, dto.RawgId);
            Assert.Equal("test-game", dto.RawgSlug);
            Assert.Equal("Test Game", dto.RawgName);
            Assert.Equal("Test description", dto.RawgDescription);
            Assert.Equal(88, dto.RawgMetacritic);
            Assert.Equal("2024-01-01", dto.RawgReleased);
            Assert.Equal("https://example.com/image.jpg", dto.RawgBackgroundImage);
            Assert.Equal("https://example.com", dto.RawgWebsite);
            Assert.Equal(["PC", "PlayStation 5"], dto.RawgPlatforms);
            Assert.NotNull(dto.RawgRequirements);
            Assert.Equal("Minimum specs", dto.RawgRequirements!.Minimum);
            Assert.Equal("Recommended specs", dto.RawgRequirements.Recommended);
            Assert.Null(dto.RawgRequirements.High);
            Assert.Null(dto.RawgRequirements.VeryHigh);
            Assert.Null(dto.RawgRequirements.Ultra);
        }

        private sealed class FakeRawgApiClient : IRawgApiClient
        {
            private readonly Result<RawgGameDetailedResponse> _gameByIdResult;

            public FakeRawgApiClient(Result<RawgGameDetailedResponse> gameByIdResult)
            {
                _gameByIdResult = gameByIdResult;
            }

            public Task<Result<RawgGameDetailedResponse>> GetGameByIdAsync(int id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_gameByIdResult);
            }

            public Task<Result<RawgSearchResponse>> SearchGamesAsync(List<string> queryParameters, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
    }
}
