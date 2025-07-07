using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.EntityFrameworkCore.Repository
{
    using Microsoft.EntityFrameworkCore.Storage;
    using System.Linq.Expressions;

    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll(bool asNoTracking = false);
        Task<List<T>> GetAllAsync(bool asNoTracking = false);

        T GetById(object id);
        Task<T> GetByIdAsync(object id);

        void Insert(T entity);
        Task InsertAsync(T entity);
        object InsertAndGetId(T entity);
        Task<object> InsertAndGetIdAsync(T entity);

        void Update(T entity);
        Task UpdateAsync(T entity);

        void Delete(T entity);
        Task DeleteAsync(T entity);
        void DeleteById(object id);
        Task DeleteByIdAsync(object id);

        object InsertOrUpdateAndGetId(T entity);
        Task<object> InsertOrUpdateAndGetIdAsync(T entity);

        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        T LastOrDefault(Expression<Func<T, bool>> predicate);
        Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate);

        IEnumerable<T> Where(Expression<Func<T, bool>> predicate);
        Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate);

        bool Any(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        int Count(Expression<Func<T, bool>> predicate = null);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

        IQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true);

        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate);

        Task<IDbContextTransaction> BeginTransactionAsync();
        IQueryable<T> GetAllIncluding(params Expression<Func<T, object>>[] includeProperties);

    }

}
