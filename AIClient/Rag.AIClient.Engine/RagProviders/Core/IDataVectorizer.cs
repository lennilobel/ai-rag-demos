using System.Threading.Tasks;

namespace Rag.AIClient.Engine.RagProviders.Core
{
	public interface IDataVectorizer
	{
		Task VectorizeData();
		Task VectorizeData(int[] movieIds = null);
	}
}
