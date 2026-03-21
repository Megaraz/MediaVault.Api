using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services.MediaEntry
{
    public class MediaEntryDtoValidator : IDtoValidator<Guid, MediaEntryCreateDto, MediaEntryUpdateDto>
    {
        public bool IsValidCreateDto(MediaEntryCreateDto createDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            validationErrors = new List<ValidationError>();

            if (ValidatorExtensions.IsNull(createDto, errorContext, out ValidationError nullValueError))
            {
                validationErrors = validationErrors.Append(nullValueError);
                return false;
            }

            errorContext.FieldName = nameof(createDto.Title);
            if (ValidatorExtensions.IsNullOrWhiteSpace(createDto.Title, errorContext, out ValidationError nullOrEmptyError))
            {
                validationErrors = validationErrors.Append(nullOrEmptyError);
            }

            return !validationErrors.Any();

        }

        public bool IsValidUpdateDto(Guid id, MediaEntryUpdateDto updateDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            validationErrors = new List<ValidationError>();

            if (!Validator.IsValidId(id))
            {
                errorContext.DescriptionSuffix = "A valid Id is required and cannot be null or empty.";
                errorContext.FieldName = nameof(id);

                var idError = ValidationError.Required(errorContext);
                validationErrors = validationErrors.Append(idError);
            }

            if (ValidatorExtensions.IsNull(updateDto, errorContext, out ValidationError nullValueError))
            {
                validationErrors = validationErrors.Append(nullValueError);
                return false;
            }

            errorContext.FieldName = nameof(updateDto.Title);
            if (ValidatorExtensions.IsNullOrWhiteSpace(updateDto.Title, errorContext, out ValidationError nullOrEmptyError))
            {
                validationErrors = validationErrors.Append(nullOrEmptyError);
            }

            return !validationErrors.Any();
        }
    }
}
