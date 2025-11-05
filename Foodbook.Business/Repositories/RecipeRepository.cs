using Foodbook.Business.Repositories.Interfaces;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Foodbook.Business.Repositories
{
	public class RecipeRepository : GenericRepository<Recipe>, IRecipeRepository
	{
		private readonly FoodbookDbContext _db;

		public RecipeRepository(FoodbookDbContext db) : base(db)
		{
			_db = db;
		}

		public Task<List<Recipe>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
		{
			return _db.Recipes.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
		}
	}
}


