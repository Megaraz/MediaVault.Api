using Rasmus.SharedKernel.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services.CrudServiceInterfaces
{
    public interface ICreateService<TCreateDto, TDetailedDto>
    {
        Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct = default);
    }
}
