using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LongRunningTasks.Core.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IQueryable<T>> All();
        Task<T> GetById(Guid id);
        bool Add(T entity);
        Task<bool> Delete(Guid id);
        Task<bool> Update(T entity);
        IQueryable<T> Find(Expression<Func<T, bool>> predicate);
        IQueryable<T> Where(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    }
}
