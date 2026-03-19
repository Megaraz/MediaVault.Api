using System;
using System.Collections.Generic;
using System.Text;
using Rasmus.SharedKernel.Interfaces;

namespace media_vault_app.Application.Mappers
{
    public interface IMapper<TEntity, TKey, TCreateDto, TUpdateDto, TDetailedDto, TCollectionDto, TMinimalDto>
        where TEntity : class, IEntityId<TKey>, new()
        where TDetailedDto : IEntityId<TKey>
    {
        public TEntity ToEntity(TCreateDto createDto);
        public TEntity ToEntity(TUpdateDto updateDto);
        public TEntity ToEntity(TDetailedDto detailedDto);
        public IEnumerable<TEntity> ToEntities(IEnumerable<TDetailedDto> detailedDtos);
        public TDetailedDto ToDetailedDTO(TEntity entity);
        public TMinimalDto ToMinimalDTO(TEntity entity);
        public TCollectionDto ToCollectionDTO(IEnumerable<TEntity> entities);
        public IEnumerable<TDetailedDto> ToDetailedDTOCollection(IEnumerable<TEntity> entities);
        public IEnumerable<TMinimalDto> ToMinimalDTOCollection(IEnumerable<TEntity> entities);
    }
}
