using Foodbook.Data.Entities;

namespace Foodbook.Business.Repositories.Interfaces
{
	public interface IRatingRepository : IRepository<Rating>
	{
		Task<Rating?> GetByUserAndRecipeAsync(int userId, int recipeId, CancellationToken cancellationToken = default);
	}
}


