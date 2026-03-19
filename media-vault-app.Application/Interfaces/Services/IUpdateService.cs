using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IUpdateService<TKey, TUpdateDto>
    {
        Task<Result> UpdateAsync(TKey id, TUpdateDto entity, CancellationToken ct);
    }
}
