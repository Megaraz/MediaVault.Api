using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IGetByIdService<TKey, TDetailedDto>
    {
        Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct);
    }
}
