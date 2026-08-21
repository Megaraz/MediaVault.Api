using media_vault_app.Application.DTOs.User.Request;
using Rasmus.SharedKernel.Validation;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Validators
{
    public interface IUserDtoValidator
    {
        bool IsValidLoginDto(UserLoginDto loginDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors);
        bool IsValidCreateDto(UserRegisterDto createDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors);
        bool IsValidUpdateDto(UserUpdateDto updateDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors);
    }
}
