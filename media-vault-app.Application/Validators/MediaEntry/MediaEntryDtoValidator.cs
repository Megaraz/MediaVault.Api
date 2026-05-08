using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Validators.MediaEntry
{
    public class MediaEntryDtoValidator : IMediaEntryDtoValidator
    {
        public bool IsValidCreateDto(MediaEntryCreateDto createDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            List<ValidationError> internalErrors = new();

            if (ValidatorExtensions.IsNull(createDto, errorContext, out ValidationError nullValueError))
            {
                internalErrors.Add(nullValueError);
                validationErrors = internalErrors;
                return false;
            }

            var titleErrorContext = errorContext with { FieldName = nameof(createDto.Title) };

            if (ValidatorExtensions.IsNullOrWhiteSpace(createDto.Title, titleErrorContext, out ValidationError nullOrEmptyError))
            {
                internalErrors.Add(nullOrEmptyError);
            }

            validationErrors = internalErrors;
            return internalErrors.Count == 0;

        }

        public bool IsValidUpdateDto(MediaEntryUpdateDto updateDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors)
        {
            List<ValidationError> internalErrors = new();

            if (updateDto.IsNull(errorContext, out ValidationError nullValueError))
            {
                internalErrors.Add(nullValueError);
                validationErrors = internalErrors;
                return false;
            }

            var titleErrorContext = errorContext with { FieldName = nameof(updateDto.Title) };

            if (updateDto.Title.IsNullOrWhiteSpace(titleErrorContext, out ValidationError nullOrEmptyError))
            {
                internalErrors.Add(nullOrEmptyError);
            }

            validationErrors= internalErrors;
            return internalErrors.Count == 0;
        }

    }
}
