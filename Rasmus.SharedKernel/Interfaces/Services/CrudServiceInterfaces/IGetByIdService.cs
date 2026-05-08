using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services.CrudServiceInterfaces
{
    public interface IGetByIdService<TKey, TDetailedDto>
    {
        Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct = default);
    }
}
