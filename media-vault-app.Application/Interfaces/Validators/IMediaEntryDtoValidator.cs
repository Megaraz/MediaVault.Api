using media_vault_app.Application.DTOs.MediaEntry.Request;
using Megaraz.ResultPattern;

namespace media_vault_app.Application.Interfaces.Validators
{
    public interface IMediaEntryDtoValidator
    {
        bool IsValidCreateDto(
            MediaEntryCreateDto createDto,
            ErrorContext errorContext,
            out IReadOnlyList<ValidationError> validationErrors);

        bool IsValidUpdateDto(
            MediaEntryUpdateDto updateDto,
            ErrorContext errorContext,
            out IReadOnlyList<ValidationError> validationErrors);
    }
}
