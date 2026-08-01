using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.Interfaces.Validators;
using Megaraz.ResultPattern;
using Rasmus.SharedKernel.Validation;

namespace media_vault_app.Application.Validators.MediaEntry
{
    public class MediaEntryDtoValidator : IMediaEntryDtoValidator
    {
        public bool IsValidCreateDto(MediaEntryCreateDto createDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors)
        {
            List<ValidationError> internalErrors = new();

            if (MediaVaultValidatorExtensions.IsMediaVaultNull(createDto, errorContext, out ValidationError nullValueError))
            {
                internalErrors.Add(nullValueError);
                validationErrors = internalErrors;
                return false;
            }

            var titleErrorContext = errorContext with { FieldName = nameof(createDto.Title) };

            if (MediaVaultValidatorExtensions.IsMissingMediaVaultValue(createDto.Title, titleErrorContext, out ValidationError nullOrEmptyError))
            {
                internalErrors.Add(nullOrEmptyError);
            }

            validationErrors = internalErrors;
            return internalErrors.Count == 0;

        }

        public bool IsValidUpdateDto(MediaEntryUpdateDto updateDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors)
        {
            List<ValidationError> internalErrors = new();

            if (updateDto.IsMediaVaultNull(errorContext, out ValidationError nullValueError))
            {
                internalErrors.Add(nullValueError);
                validationErrors = internalErrors;
                return false;
            }

            var titleErrorContext = errorContext with { FieldName = nameof(updateDto.Title) };

            if (updateDto.Title.IsMissingMediaVaultValue(titleErrorContext, out ValidationError nullOrEmptyError))
            {
                internalErrors.Add(nullOrEmptyError);
            }

            validationErrors = internalErrors;
            return internalErrors.Count == 0;
        }

    }
}
