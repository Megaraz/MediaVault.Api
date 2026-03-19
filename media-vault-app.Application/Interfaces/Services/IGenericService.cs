using System;
using System.Collections.Generic;
using System.Text;
using media_vault_app.Application.Mappers;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Interfaces.Services
{
    public interface IGenericService<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto, TMinimalDto>
        where TEntity : class, IEntityId<TKey>, new()
        where TDetailedDto : IEntityId<TKey>
    {
        //IMapper<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto, TCollectionDto, TMinimalDto> Mapper { get; }

        //IGenericRepoEFCore<TEntity, TKey> Repo { get; }

        Task<Result<TDetailedDto>> CreateAsync(TCreateDto entity, CancellationToken ct);
        Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct);
        Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<Result<IEnumerable<TMinimalDto>>> GetMinimalCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default);
        Task<Result<IEnumerable<TDetailedDto>>> SearchAsync(string searchTerm, CancellationToken ct = default);
        Task<Result> UpdateAsync(TKey id, TUpdateDto entity, CancellationToken ct);
        Task<Result> DeleteAsync(TKey Id, CancellationToken ct);
    }
}
