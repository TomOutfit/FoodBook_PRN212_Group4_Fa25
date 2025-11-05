using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Foodbook.Business.Repositories.Interfaces
{
	public interface IRepository<TEntity> where TEntity : class
	{
		DbContext Context { get; }
		DbSet<TEntity> Set { get; }

		Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
		Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
		IQueryable<TEntity> Query(Expression<Func<TEntity, bool>>? predicate = null);
		Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
		Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
		Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
		Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}


