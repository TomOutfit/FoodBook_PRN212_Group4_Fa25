using Foodbook.Business.Repositories.Interfaces;
using Foodbook.Data;
using Foodbook.Data.Entities;

namespace Foodbook.Business.Repositories
{
	public class LogEntryRepository : GenericRepository<LogEntry>, ILogEntryRepository
	{
		public LogEntryRepository(FoodbookDbContext db) : base(db)
		{
		}
	}
}


