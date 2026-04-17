using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Validators;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Validators.MediaEntry
{
    public class MediaEntryDtoValidator : IDtoValidator<Guid, MediaEntryCreateDto, MediaEntryUpdateDto>
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

            //validationErrors = ValidateRating(createDto.Rating, errorContext, internalErrors);
            validationErrors = internalErrors;
            return !validationErrors.Any();

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

            //validationErrors = ValidateRating(updateDto.Rating, errorContext, internalErrors);
            validationErrors= internalErrors;
            return !validationErrors.Any();
        }

        private void CheckAndFixRating(ref decimal rating)
        {
            var clamped = Math.Clamp(rating, 0m, 5m);
            rating = Math.Round(clamped * 2, MidpointRounding.AwayFromZero) / 2;
        }

        private static List<ValidationError> ValidateRating(decimal rating, ErrorContext errorContext, List<ValidationError> validationErrors)
        {
            var ratingErrorContext = errorContext with { FieldName = "Rating" };

            if (rating < 0m || rating > 5m)
            {
                validationErrors.Add(ValidationError.OutOfRange(ratingErrorContext, "0 to 5"));
            }

            if (rating * 2m != decimal.Truncate(rating * 2m))
            {
                validationErrors.Add(ValidationError.InvalidFormat(ratingErrorContext, "0.5 increments between 0 and 5"));
            }

            return validationErrors;
        }
    }
}
