using UserManagement.EntityFrameworkCore.Context;

namespace UserManagement.EntityFrameworkCore.Repository
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using System.Linq.Expressions;

    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> GetAll(bool asNoTracking = false)
            => asNoTracking ? _dbSet.AsNoTracking() : _dbSet;

        public async Task<List<T>> GetAllAsync(bool asNoTracking = false)
            => await GetAll(asNoTracking).ToListAsync();

        public T GetById(object id) => _dbSet.Find(id);

        public async Task<T> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

        public void Insert(T entity) => _dbSet.Add(entity);

        public async Task InsertAsync(T entity) => await _dbSet.AddAsync(entity);

        public object InsertAndGetId(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
            return GetPrimaryKey(entity);
        }

        public async Task<object> InsertAndGetIdAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return GetPrimaryKey(entity);
        }

        public void Update(T entity) => _dbSet.Update(entity);

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public void Delete(T entity) => _dbSet.Remove(entity);

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public void DeleteById(object id)
        {
            var entity = _dbSet.Find(id);
            if (entity != null) _dbSet.Remove(entity);
        }

        public async Task DeleteByIdAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null) _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public object InsertOrUpdateAndGetId(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry.IsKeySet)
                _dbSet.Update(entity);
            else
                _dbSet.Add(entity);

            _context.SaveChanges();
            return GetPrimaryKey(entity);
        }

        public async Task<object> InsertOrUpdateAndGetIdAsync(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry.IsKeySet)
                _dbSet.Update(entity);
            else
                await _dbSet.AddAsync(entity);

            await _context.SaveChangesAsync();
            return GetPrimaryKey(entity);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
            => _dbSet.FirstOrDefault(predicate);

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        public T LastOrDefault(Expression<Func<T, bool>> predicate)
            => _dbSet.Where(predicate).ToList().LastOrDefault();

        public async Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var list = await _dbSet.Where(predicate).ToListAsync();
            return list.LastOrDefault();
        }

        public IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
            => _dbSet.Where(predicate);

        public async Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public bool Any(Expression<Func<T, bool>> predicate)
            => _dbSet.Any(predicate);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        public int Count(Expression<Func<T, bool>> predicate = null)
            => predicate == null ? _dbSet.Count() : _dbSet.Count(predicate);

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
            => predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

        public IQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true)
            => ascending ? _dbSet.OrderBy(keySelector) : _dbSet.OrderByDescending(keySelector);

        public int SaveChanges() => _context.SaveChanges();

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        private object GetPrimaryKey(T entity)
        {
            var key = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.First();
            return typeof(T).GetProperty(key.Name)?.GetValue(entity);
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public IQueryable<T> GetAllIncluding(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return query;
        }
    }
}