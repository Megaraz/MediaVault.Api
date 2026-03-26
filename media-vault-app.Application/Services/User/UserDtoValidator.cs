using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.User.Request;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.User
{
    public class UserDtoValidator : IDtoValidator<Guid, UserRegisterDto>
    {
        public bool IsValidLoginDto(UserLoginDto loginDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            validationErrors = new List<ValidationError>();
            var internalValidationErrors = new List<ValidationError>();

            if (loginDto.IsNull(errorContext, out ValidationError nullValueError))
            {
                internalValidationErrors.Add(nullValueError);
                validationErrors = internalValidationErrors;
                return false;
            }

            var requiredFields = new (string FieldName, string Value)[]
            {
                ("Username or Email", loginDto.UsernameOrEmail),
                ("Password", loginDto.Password)
            };

            if (requiredFields.AnyIsNullOrWhiteSpace(errorContext, out IEnumerable<ValidationError> nullOrEmptyErrors))
            {
                internalValidationErrors.AddRange(nullOrEmptyErrors);
            }

            validationErrors = internalValidationErrors;
            return !validationErrors.Any();
        }
        public bool IsValidCreateDto(UserRegisterDto createDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            validationErrors = new List<ValidationError>();
            var internalErrors = new List<ValidationError>();

            if (createDto.IsNull(errorContext, out ValidationError nullValueError))
            {
                internalErrors.Add(nullValueError);
                validationErrors = internalErrors;
                return false;
            }

            var requiredFields = new (string FieldName, string Value)[]
            {
                (nameof(createDto.Username), createDto.Username),
                (nameof(createDto.Email), createDto.Email),
                (nameof(createDto.ConfirmEmail), createDto.ConfirmEmail),
                (nameof(createDto.Password), createDto.Password),
                (nameof(createDto.ConfirmPassword), createDto.ConfirmPassword)
            };

            if (requiredFields.AnyIsNullOrWhiteSpace(errorContext, out IEnumerable<ValidationError> nullOrEmptyErrors))
            {
                internalErrors.AddRange(nullOrEmptyErrors);
            }

            if (!createDto.Email.Matches(createDto.ConfirmEmail, errorContext, out ValidationError notMatchingEmailError))
            {
                internalErrors.Add(notMatchingEmailError);
            }

            if (!createDto.Password.Matches(createDto.ConfirmPassword, errorContext, out ValidationError notMatchingPasswordError))
            {
                internalErrors.Add(notMatchingPasswordError);
            }

            validationErrors = internalErrors;
            return !validationErrors.Any();
        }

        // TODO: Implement update DTO validation logic
        public bool IsValidUpdateDto(UserUpdateDto updateDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            throw new NotImplementedException();
        }
    }
}
