using System.Threading.Tasks;

namespace Rag.AIClient.Engine.RagProviders.Core
{
	public interface IDataPopulator
	{
		Task InitializeData();
		Task ResetData();
		Task UpdateData();
	}
}
