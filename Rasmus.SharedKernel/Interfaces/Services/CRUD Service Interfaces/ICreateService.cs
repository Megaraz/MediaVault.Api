using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services
{
    public interface ICreateService<TCreateDto, TDetailedDto>
    {
        Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct);
    }
}
