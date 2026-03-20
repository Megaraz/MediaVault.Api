using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.User
{
    public class UserDtoValidator : IDtoValidator<Guid, UserCreateDto, UserUpdateDto>
    {
        public bool IsValidLoginDto(UserLoginDto loginDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            validationErrors = new List<ValidationError>();

            if (loginDto is null)
            {
                errorContext.DescriptionSuffix = $"A value for the entity '{errorContext.EntityName}' is required and cannot be null or empty.";

                ValidationError nullValueError = ValidationError.Required<UserLoginDto>(errorContext);
                validationErrors.Append(nullValueError);
                return false;
            }

            if (string.IsNullOrWhiteSpace(loginDto.UsernameOrEmail))
            {
                errorContext.DescriptionSuffix = $"The field 'UsernameOrEmail' is required for the entity '{errorContext.EntityName}' and cannot be null or empty.";
                ValidationError nullValueError = ValidationError.Required<UserLoginDto>(errorContext);
                validationErrors.Append(nullValueError);
                return false;
            }

            if (string.IsNullOrWhiteSpace(loginDto.Password))
            {
                errorContext.DescriptionSuffix = $"The field 'Password' is required for the entity '{errorContext.EntityName}' and cannot be null or empty.";
                ValidationError nullValueError = ValidationError.Required<UserLoginDto>(errorContext);
                validationErrors.Append(nullValueError);
                return false;
            }

            return !validationErrors.Any();
        }
        public bool IsValidCreateDto(UserCreateDto createDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            validationErrors = new List<ValidationError>();

            if (createDto is null)
            {
                errorContext.DescriptionSuffix = $"A value for the entity '{errorContext.EntityName}' is required and cannot be null or empty.";

                ValidationError nullValueError = ValidationError.Required<UserCreateDto>(errorContext);

                validationErrors.Append(nullValueError);
                return false;

            }

            if (!string.Equals(createDto.Email, createDto.ConfirmEmail, StringComparison.OrdinalIgnoreCase))
            {
                errorContext.DescriptionSuffix = $"Email and ConfirmEmail must match for the entity '{errorContext.EntityName}'.";

                validationErrors.Append(
                    ValidationError.Custom<UserCreateDto>(errorContext));
            }

            if (!string.Equals(createDto.Password, createDto.ConfirmPassword, StringComparison.Ordinal))
            {
                errorContext.DescriptionSuffix = $"Password and ConfirmPassword must match for the entity '{errorContext.EntityName}'.";

                validationErrors.Append(
                    ValidationError.Custom<UserCreateDto>(errorContext));
            }

            return !validationErrors.Any();
        }

        public bool IsValidUpdateDto(Guid id, UserUpdateDto updateDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            throw new NotImplementedException();
        }
    }
}
