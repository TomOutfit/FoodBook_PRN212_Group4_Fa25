using Foodbook.Business.Repositories.Interfaces;
using Foodbook.Data;
using Foodbook.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Foodbook.Business.Repositories
{
	public class UserRepository : GenericRepository<User>, IUserRepository
	{
		private readonly FoodbookDbContext _db;

		public UserRepository(FoodbookDbContext db) : base(db)
		{
			_db = db;
		}

		public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
		{
			return _db.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
		}

		public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			return _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
		}
	}
}


