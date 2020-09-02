using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mwp.Repository
{
    public abstract class BaseRepository<TEntity> where TEntity : class
    {
        protected AzureKeyRotationDbContext DbContext;
        protected DbSet<TEntity> DbSet;

        protected BaseRepository(AzureKeyRotationDbContext dbContext)
        {
            DbContext = dbContext;
            DbSet = DbContext.Set<TEntity>();
        }

        public virtual Task<IQueryable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = DbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            return Task.FromResult(query);
        }

        public virtual async Task UpdateRange(IEnumerable<TEntity> entities, bool autoSave)
        {
            DbSet.UpdateRange(entities);
            if (autoSave)
            {
                await SaveChangesAsync();
            }
        }

        public virtual async Task SaveChangesAsync()
        {
            await DbContext.SaveChangesAsync();
        }
    }
}