using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface ICreateService<TCreateDto, TDetailedDto>
    {
        Task<Result<TDetailedDto>> CreateAsync(TCreateDto entity, CancellationToken ct);
    }
}
