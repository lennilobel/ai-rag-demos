using System.Threading.Tasks;

namespace Rag.SqlDatabasePublisher.Base
{
	public interface IDatabasePublisher
	{
		Task Publish();
	}
}
