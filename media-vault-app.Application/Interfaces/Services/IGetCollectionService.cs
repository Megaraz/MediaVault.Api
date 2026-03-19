using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IGetCollectionService<TDetailedDto, TMinimalDto>
    {
        Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<Result<IEnumerable<TMinimalDto>>> GetMinimalCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    }
}
