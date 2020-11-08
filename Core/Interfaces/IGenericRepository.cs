using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        List<T> GetAll(params string[] includes);
        List<T> Get(Func<T, bool> filter = null, params string[] includes);
        T GetById(object id, params string[] includes);
        void Insert(T entity);
        void Update(T newEntity , T oldEntity);
        void Delete(T entity);
        bool IsExists(Guid id);

    }
}
