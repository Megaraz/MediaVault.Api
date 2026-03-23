using System;
using media_vault_app.Application.DTOs.MediaEntry.Request;
using media_vault_app.Application.DTOs.MediaEntry.Response;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IMediaEntryWriteService
    {
        Task<Result<MediaEntryDetailedDto>> CreateAsync(Guid userId, MediaEntryCreateDto createDto, CancellationToken ct = default);
        Task<Result> UpdateAsync(Guid userId, Guid mediaEntryId, MediaEntryUpdateDto updateDto, CancellationToken ct = default);
        Task<Result> DeleteAsync(Guid userId, Guid mediaEntryId, CancellationToken ct = default);
    }
}
