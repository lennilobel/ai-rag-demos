using System.Threading.Tasks;

namespace Rag.SqlDatabasePublisher.Core
{
	public interface IDatabasePublisher
	{
		Task Publish(DatabasePublisherConfig config);
	}
}
