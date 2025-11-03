using Foodbook.Data.Entities;

namespace Foodbook.Business.Repositories.Interfaces
{
	public interface IRecipeRepository : IRepository<Recipe>
	{
		Task<List<Recipe>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
	}
}


