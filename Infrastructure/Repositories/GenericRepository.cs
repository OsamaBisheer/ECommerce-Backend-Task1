using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        //
        private readonly DataContext _context;
        private DbSet<T> table = null;

        public GenericRepository(DataContext context)
        {
            _context = context;
            table = _context.Set<T>();
        }

        public void Delete(T obj)
        {
            table.Remove(obj);
        }

        public virtual List<T> Get(Func<T, bool> filter = null, params string[] includes)
        {
            var firstRes = GetAll(includes);

            if (filter == null)
                return firstRes;

            return firstRes.Where(filter).ToList();
        }

        public List<T> GetAll(params string[] includes)
        {
            string inculding = "";

            for (int i = 0; i < includes.Length; i++)
            {
                if (i == includes.Length - 1)
                {
                    inculding += includes[i];
                }
                else
                {
                    inculding += includes[i] + ",";
                }
            }

            if (inculding == "")
                return table.ToList();

            return table.Include(inculding).ToList();
        }

        public virtual T GetById(object id, params string[] includes)
        {
            string inculding = "";

            for (int i = 0; i < includes.Length; i++)
            {
                if (i == includes.Length - 1)
                {
                    inculding += includes[i];
                }
                else
                {
                    inculding += includes[i] + ",";
                }
            }

            var res = table.Find(id);

            if (inculding != "")
            {
                _context.Entry(res).Collection(inculding).Load();
            }

            return res;
        }

        public void Insert(T entity)
        {
            table.Add(entity);
        }

        public void Update(T newEntity, T oldEntity)
        {
            _context.Entry(oldEntity).State = EntityState.Detached;
            table.Attach(newEntity);
            _context.Entry(newEntity).State = EntityState.Modified;
        }

        public bool IsExists(Guid id)
        {
           return GetById(id) != null;
        }

        //public List<T> Search(string term)
        //{
        //    var res = table.Where(b=>b.)
        //}

    }
}
