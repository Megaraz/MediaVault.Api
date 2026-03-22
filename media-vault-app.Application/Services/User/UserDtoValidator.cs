using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.User
{
    public class UserDtoValidator : IDtoValidator<Guid, UserCreateDto>
    {
        public bool IsValidLoginDto(UserLoginDto loginDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            validationErrors = new List<ValidationError>();

            if (loginDto.IsNull(errorContext, out ValidationError nullValueError))
            {
                validationErrors = validationErrors.Append(nullValueError);
                return false;
            }

            if (loginDto.UsernameOrEmail.IsNullOrWhiteSpace(errorContext, out ValidationError nullOrEmptyError))
            {
                validationErrors = validationErrors.Append(nullOrEmptyError);
            }

            if (loginDto.Password.IsNullOrWhiteSpace(errorContext, out ValidationError nullOrEmptyPasswordError))
            {
                validationErrors = validationErrors.Append(nullOrEmptyPasswordError);
            }

            return !validationErrors.Any();
        }
        public bool IsValidCreateDto(UserCreateDto createDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            validationErrors = new List<ValidationError>();

            if (createDto.IsNull(errorContext, out ValidationError nullValueError))
            {
                validationErrors = validationErrors.Append(nullValueError);
                return false;
            }

            string[] requiredFields = { createDto.Username, createDto.Email, createDto.ConfirmEmail, createDto.Password, createDto.ConfirmPassword };

            if (requiredFields.AnyIsNullOrWhiteSpace(errorContext, out ValidationError nullOrEmptyError))
            {
                validationErrors = validationErrors.Append(nullOrEmptyError);
            }

            if (!createDto.Email.Matches(createDto.ConfirmEmail, errorContext, out ValidationError notMatchingEmailError))
            {
                validationErrors = validationErrors.Append(notMatchingEmailError);
            }

            if (!createDto.Password.Matches(createDto.ConfirmPassword, errorContext, out ValidationError notMatchingPasswordError))
            {
                validationErrors = validationErrors.Append(notMatchingPasswordError);
            }

            return !validationErrors.Any();
        }

        public bool IsValidUpdateDto(UserUpdateDto updateDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            throw new NotImplementedException();
        }
    }
}
