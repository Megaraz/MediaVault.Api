using System.Security.Claims;
using media_vault_app.API.Controllers;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Mappers.User;
using media_vault_app.Application.Services.User;
using media_vault_app.Application.Validators.User;
using media_vault_app.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace media_vault_app.Tests.API.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task UpdateUser_UsesProfilePath_AndReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var userRepo = new FakeUserRepo();
        var userWriteService = new UserWriteService(
            userRepo,
            new UserEntityMapper(),
            new UserDtoMapper(),
            new UserDtoValidator(),
            ServiceTestLogger.Create<UserWriteService>());
        var controller = new AuthController(
            null!,
            null!,
            userWriteService,
            null!)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                        [
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                        ],
                        "test"))
                }
            }
        };

        var result = await controller.UpdateUser(
            new UserUpdateDto
            {
                UserName = "updated-user",
                Email = "updated@example.com"
            });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(1, userRepo.ProfileUpdateCallCount);
        Assert.Equal(0, userRepo.UpdateCallCount);
        Assert.Equal(userId, userRepo.LastProfileUpdateRequest!.Value.UserId);
    }
}
