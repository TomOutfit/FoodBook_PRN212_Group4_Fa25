using Foodbook.Data.Entities;

namespace Foodbook.Business.Repositories.Interfaces
{
	public interface IIngredientRepository : IRepository<Ingredient>
	{
		Task<List<Ingredient>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
	}
}


