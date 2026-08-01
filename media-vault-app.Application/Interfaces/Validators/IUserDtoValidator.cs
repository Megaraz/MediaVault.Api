using media_vault_app.Application.DTOs.User.Request;
using Rasmus.SharedKernel.Interfaces.Validators;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Validators
{
    public interface IUserDtoValidator : IDtoValidator<Guid, UserRegisterDto, UserUpdateDto>
    {
        bool IsValidLoginDto(UserLoginDto loginDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors);
    }
}
