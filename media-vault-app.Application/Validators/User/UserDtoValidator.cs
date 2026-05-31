using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using media_vault_app.Application.Interfaces.Validators;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Validators.User
{
    public class UserDtoValidator : IUserDtoValidator
    {
        public bool IsValidLoginDto(UserLoginDto loginDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors)
        {
            var localValidationErrors = new List<ValidationError>();

            if (loginDto.IsNull(errorContext, out ValidationError nullValueError))
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

            if (requiredFields.RequiredFieldsAreNullOrWhiteSpace(errorContext, out IReadOnlyList<ValidationError> nullOrEmptyErrors))
            {
                localValidationErrors.AddRange(nullOrEmptyErrors);
            }

            validationErrors = localValidationErrors;
            return !validationErrors.Any();
        }
        public bool IsValidCreateDto(UserRegisterDto createDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors)
        {
            var internalErrors = new List<ValidationError>();

            if (createDto.IsNull(errorContext, out ValidationError nullValueError))
            {
                internalErrors.Add(nullValueError);
                validationErrors = internalErrors;
                return false;
            }

            var requiredFields = new (string FieldName, string? Value)[]
            {
                (nameof(createDto.Username), createDto.Username),
                (nameof(createDto.Email), createDto.Email),
                (nameof(createDto.ConfirmEmail), createDto.ConfirmEmail),
                (nameof(createDto.Password), createDto.Password),
                (nameof(createDto.ConfirmPassword), createDto.ConfirmPassword)
            };

            if (requiredFields.RequiredFieldsAreNullOrWhiteSpace(errorContext, out IReadOnlyList<ValidationError> nullOrEmptyErrors))
            {
                internalErrors.AddRange(nullOrEmptyErrors);
            }

            if (!string.IsNullOrWhiteSpace(createDto.Email) && !string.IsNullOrWhiteSpace(createDto.ConfirmEmail))
            {
                if (createDto.Email.DoesNotMatch(createDto.ConfirmEmail, nameof(createDto.Email), nameof(createDto.ConfirmEmail), errorContext, out ValidationError notMatchingEmailError))
                {
                    internalErrors.Add(notMatchingEmailError);
                }
            }

            if (!string.IsNullOrWhiteSpace(createDto.Password) && !string.IsNullOrWhiteSpace(createDto.ConfirmPassword))
            {
                if (createDto.Password.DoesNotMatch(createDto.ConfirmPassword, nameof(createDto.Password), nameof(createDto.ConfirmPassword), errorContext, out ValidationError notMatchingPasswordError))
                {
                    internalErrors.Add(notMatchingPasswordError);
                }
            }

            validationErrors = internalErrors;
            return !validationErrors.Any();
        }

        // TODO: Implement update DTO validation logic
        public bool IsValidUpdateDto(UserUpdateDto updateDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors)
        {
            throw new NotImplementedException();
        }

    }
}
