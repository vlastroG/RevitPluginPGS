using MS.Commands.Models.Interfaces;
using MS.Commands.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.Services.Base
{
    public abstract class RepositoryInMemory<T> : IRepository<T> where T : IEntity
    {
        private readonly List<T> _entities = new List<T>();

        private int _lastId;


        protected RepositoryInMemory()
        {

        }

        protected RepositoryInMemory(IEnumerable<T> entities)
        {
            foreach (var item in entities)
            {
                Add(item);
            }
        }

        public bool Add(T item)
        {
            if (Equals(item, null) || _entities.Contains(item))
            {
                return false;
            }
            else
            {
                item.Id = ++_lastId;
                _entities.Add(item);
                return true;
            }
        }

        public bool Delete(T item)
        {
            if (Equals(item, null))
            {
                return false;
            }
            else
            {
                return _entities.Remove(item);
            }
        }

        public T Get(int id)
        {
            return GetAll().FirstOrDefault(item => item.Id == id);
        }

        public IEnumerable<T> GetAll() => _entities;

        public bool Update(int id, T item)
        {
            if (Equals(item, null) || id <= 0 || _entities.Contains(item))
            {
                return false;
            }
            else
            {
                var dbItem = ((IRepository<T>)this).Get(id);
                if (dbItem.Equals(null))
                {
                    return false;
                }
                else
                {
                    return Update(dbItem, item);
                }
            }
        }

        protected abstract bool Update(T source, T destination);
    }
}
