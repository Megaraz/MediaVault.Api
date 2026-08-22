using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Interfaces.Validators;
using media_vault_app.Application.Validation;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Validators.User
{
    public class UserDtoValidator : IUserDtoValidator
    {
        public bool IsValidLoginDto(UserLoginDto loginDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors)
        {
            var localValidationErrors = new List<ValidationError>();

            if (loginDto.IsMediaVaultNull(errorContext, out ValidationError nullValueError))
            {
                localValidationErrors.Add(nullValueError);
                validationErrors = localValidationErrors;
                return false;
            }

            var requiredFields = new (string FieldName, string? Value)[]
            {
                ("Username or Email", loginDto.UsernameOrEmail),
                ("Password", loginDto.Password)
            };

            if (requiredFields.HasMissingRequiredFields(errorContext, out IReadOnlyList<ValidationError> nullOrEmptyErrors))
            {
                localValidationErrors.AddRange(nullOrEmptyErrors);
            }

            validationErrors = localValidationErrors;
            return !validationErrors.Any();
        }
        public bool IsValidCreateDto(UserRegisterDto createDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors)
        {
            var internalErrors = new List<ValidationError>();

            if (createDto.IsMediaVaultNull(errorContext, out ValidationError nullValueError))
            {
                internalErrors.Add(nullValueError);
                validationErrors = internalErrors;
                return false;
            }

            MediaVaultWriteValidation.AddText(
                internalErrors,
                createDto.Username,
                errorContext,
                nameof(createDto.Username),
                MediaVaultWriteValidationPolicy.UserNameMaxLength,
                required: true);
            MediaVaultWriteValidation.AddEmail(
                internalErrors,
                createDto.Email,
                errorContext,
                nameof(createDto.Email),
                required: true);
            MediaVaultWriteValidation.AddEmail(
                internalErrors,
                createDto.ConfirmEmail,
                errorContext,
                nameof(createDto.ConfirmEmail),
                required: true);
            MediaVaultWriteValidation.AddText(
                internalErrors,
                createDto.Password,
                errorContext,
                nameof(createDto.Password),
                MediaVaultWriteValidationPolicy.PasswordMaxLength,
                required: true);
            MediaVaultWriteValidation.AddText(
                internalErrors,
                createDto.ConfirmPassword,
                errorContext,
                nameof(createDto.ConfirmPassword),
                MediaVaultWriteValidationPolicy.PasswordMaxLength,
                required: true);

            if (!string.IsNullOrWhiteSpace(createDto.Email) && !string.IsNullOrWhiteSpace(createDto.ConfirmEmail))
            {
                if (createDto.Email.HasNonMatchingMediaVaultValues(createDto.ConfirmEmail, nameof(createDto.Email), nameof(createDto.ConfirmEmail), errorContext, out ValidationError notMatchingEmailError))
                {
                    internalErrors.Add(notMatchingEmailError);
                }
            }

            if (!string.IsNullOrWhiteSpace(createDto.Password) && !string.IsNullOrWhiteSpace(createDto.ConfirmPassword))
            {
                if (createDto.Password.HasNonMatchingMediaVaultValues(createDto.ConfirmPassword, nameof(createDto.Password), nameof(createDto.ConfirmPassword), errorContext, out ValidationError notMatchingPasswordError))
                {
                    internalErrors.Add(notMatchingPasswordError);
                }
            }

            validationErrors = internalErrors;
            return !validationErrors.Any();
        }

        public bool IsValidUpdateDto(UserUpdateDto updateDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors)
        {
            var internalErrors = new List<ValidationError>();

            if (updateDto.IsMediaVaultNull(errorContext, out ValidationError nullValueError))
            {
                internalErrors.Add(nullValueError);
                validationErrors = internalErrors;
                return false;
            }

            MediaVaultWriteValidation.AddText(
                internalErrors,
                updateDto.UserName,
                errorContext,
                nameof(updateDto.UserName),
                MediaVaultWriteValidationPolicy.UserNameMaxLength,
                required: true);
            MediaVaultWriteValidation.AddEmail(
                internalErrors,
                updateDto.Email,
                errorContext,
                nameof(updateDto.Email),
                required: true);
            MediaVaultWriteValidation.AddIntegerRange(
                internalErrors,
                updateDto.ExpectedVersion,
                errorContext,
                nameof(updateDto.ExpectedVersion),
                1,
                int.MaxValue - 1);

            validationErrors = internalErrors;
            return !validationErrors.Any();
        }

    }
}
