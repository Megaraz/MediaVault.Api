using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Validators
{
    public interface IDtoValidator<TKey, TCreateDto, TUpdateDto>
    {
        bool IsValidRegisterDto(TCreateDto createDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors);
        bool IsValidUpdateDto(TUpdateDto updateDto, ErrorContext errorContext, out IEnumerable<ValidationError> validationErrors);

    }
}
