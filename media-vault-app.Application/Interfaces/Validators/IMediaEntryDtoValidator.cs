using media_vault_app.Application.DTOs.MediaEntry.Request;
using Rasmus.SharedKernel.Interfaces.Validators;

namespace media_vault_app.Application.Interfaces.Validators
{
    public interface IMediaEntryDtoValidator : IDtoValidator<Guid, MediaEntryCreateDto, MediaEntryUpdateDto>
    {
    }
}
