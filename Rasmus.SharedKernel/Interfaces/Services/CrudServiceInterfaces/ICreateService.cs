using Megaraz.ResultPattern;

namespace Rasmus.SharedKernel.Interfaces.Services.CrudServiceInterfaces
{
    public interface ICreateService<TCreateDto, TDetailedDto>
        where TDetailedDto : notnull
    {
        Task<Result<TDetailedDto>> CreateAsync(TCreateDto createDto, CancellationToken ct = default);
    }
}
