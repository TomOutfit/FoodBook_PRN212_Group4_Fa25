using Foodbook.Business.Repositories.Interfaces;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Foodbook.Business.Repositories
{
	public class RatingRepository : GenericRepository<Rating>, IRatingRepository
	{
		private readonly FoodbookDbContext _db;

		public RatingRepository(FoodbookDbContext db) : base(db)
		{
			_db = db;
		}

		public Task<Rating?> GetByUserAndRecipeAsync(int userId, int recipeId, CancellationToken cancellationToken = default)
		{
			return _db.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.RecipeId == recipeId, cancellationToken);
		}
	}
}


