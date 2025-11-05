using System.Linq.Expressions;
using Foodbook.Business.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Foodbook.Business.Repositories
{
	public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
	{
		public DbContext Context { get; }
		public DbSet<TEntity> Set { get; }

		public GenericRepository(DbContext context)
		{
			Context = context;
			Set = context.Set<TEntity>();
		}

		public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
		{
			return await Set.FindAsync(new object?[] { id }, cancellationToken);
		}

		public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await Set.ToListAsync(cancellationToken);
		}

		public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? predicate = null)
		{
			return predicate == null ? Set.AsQueryable() : Set.Where(predicate);
		}

		public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			await Set.AddAsync(entity, cancellationToken);
			return entity;
		}

		public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
		{
			await Set.AddRangeAsync(entities, cancellationToken);
		}

		public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			Set.Update(entity);
			return Task.CompletedTask;
		}

		public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			Set.Remove(entity);
			return Task.CompletedTask;
		}

		public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return Context.SaveChangesAsync(cancellationToken);
		}
	}
}


