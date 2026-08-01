using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Validators
{
    public interface IDtoValidator<TKey, TCreateDto, TUpdateDto>
    {
        bool IsValidCreateDto(TCreateDto createDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors);
        bool IsValidUpdateDto(TUpdateDto updateDto, ErrorContext errorContext, out IReadOnlyList<ValidationError> validationErrors);

    }
}
