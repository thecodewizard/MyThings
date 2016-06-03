using System.Collections.Generic;

namespace MyThings.Common.Repositories.Interfaces
{
    public interface IGenericRepository<TEntity>
        where TEntity : class
    {
        IEnumerable<TEntity> All();
        void Delete(object id);
        void Delete(TEntity entityToDelete);
        TEntity GetByID(object id);
        TEntity Insert(TEntity entity);
        void Update(TEntity entityToUpdate);
        void SaveChanges();
    }
}