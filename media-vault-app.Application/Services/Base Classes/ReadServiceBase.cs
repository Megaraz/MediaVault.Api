using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces;
using Rasmus.SharedKernel.Interfaces.Identifiers;
using Rasmus.SharedKernel.Interfaces.Mappers.MapEntityToDto.Interfaces;
using Rasmus.SharedKernel.Interfaces.Services;
using Rasmus.SharedKernel.ResultPattern;

namespace media_vault_app.Application.Services
{
    public class ReadServiceBase<TEntity, TKey, TDetailedDto, TMinimalDto>
        : IReadService<TEntity, TKey, TDetailedDto, TMinimalDto>
            where TEntity : class, IEntityId<TKey>, new()
            where TDetailedDto : IDtoID<TKey>
            where TMinimalDto : IDtoID<TKey>
    {

        private readonly IGenericRepo<TEntity, TKey> _repo;
        private readonly IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> _entityToDtoMapper;

        public ReadServiceBase(IGenericRepo<TEntity, TKey> repo, IMapEntityToDto<TEntity, TKey, TDetailedDto, TMinimalDto> entityToDtoMapper)
        {
            _repo = repo;
            _entityToDtoMapper = entityToDtoMapper;
        }

        public Task<Result<TDetailedDto>> GetByIdAsync(TKey id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<TDetailedDto>>> GetDetailedCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<TMinimalDto>>> GetMinimalCollectionAsync(int pageNumber, int pageSize, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
