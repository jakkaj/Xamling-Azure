using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Xamling.Azure.DocumentDB
{
    public interface IDocumentConnection
    {
        Task<Database> GetDatabase(string databaseName);
        Task<DocumentCollection> GetCollection(string name, Database db);
        Task<Database> GetDatabase();
        Task<DocumentCollection> GetCollection();
        DocumentClient Client { get; }
    }
}