using Foodbook.Business.Repositories.Interfaces;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Foodbook.Business.Repositories
{
	public class IngredientRepository : GenericRepository<Ingredient>, IIngredientRepository
	{
		private readonly FoodbookDbContext _db;

		public IngredientRepository(FoodbookDbContext db) : base(db)
		{
			_db = db;
		}

		public Task<List<Ingredient>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
		{
			return _db.Ingredients.Where(i => i.UserId == userId).ToListAsync(cancellationToken);
		}
	}
}


