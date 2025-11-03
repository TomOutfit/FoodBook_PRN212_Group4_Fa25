using Foodbook.Data.Entities;

namespace Foodbook.Business.Repositories.Interfaces
{
	public interface IUserRepository : IRepository<User>
	{
		Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
		Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	}
}


