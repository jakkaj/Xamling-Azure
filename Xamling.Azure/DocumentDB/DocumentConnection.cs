using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using XamlingCore.Portable.Contract.Config;

namespace Xamling.Azure.DocumentDB
{
    public class DocumentConnection : IDocumentConnection
    {
        private readonly DocumentClient _client;
        private readonly IConfig _config;

        private Database _database;
        private DocumentCollection _documentCollection;

        public DocumentConnection(DocumentClient client, IConfig config)
        {
            _client = client;
            _config = config;
        }

       public DocumentClient Client => _client;

        public async Task<Database> GetDatabase()
        {
            if (_database == null)
            {
                _database = await GetDatabase(_config["DocumentDatabase"]);
            }

            return _database;
        }

        public async Task<bool> DeleteDatabase(string databaseId)
        {
            var db = _client.CreateDatabaseQuery()
                        .Where(d => d.Id == databaseId)
                        .AsEnumerable()
                        .FirstOrDefault();

            if (db == null)
            {
                return true;
            }

            await _client.DeleteDatabaseAsync(db.SelfLink);

            return true;
        }

        public async Task<Database> GetDatabase(string databaseId)
        {
            var db = _client.CreateDatabaseQuery()
                        .Where(d => d.Id == databaseId)
                        .AsEnumerable()
                        .FirstOrDefault();

            if(db == null)
            {
                db = await _client.CreateDatabaseAsync(new Database
                {
                    Id = databaseId
                });
            }

            return db;
        }

        public async Task<DocumentCollection> GetCollection()
        {
            if (_documentCollection == null)
            {
                var db = await GetDatabase();
                _documentCollection = await GetCollection(_config["DocumentCollection"], db);
            }

            return _documentCollection;
        }

        public async Task<DocumentCollection> GetCollection(string collectionId, Database db)
        {
            var c = _client.CreateDocumentCollectionQuery(db.CollectionsLink)
                          .Where(_ => _.Id == collectionId)
                          .AsEnumerable()
                          .FirstOrDefault();

            if (c == null)
            {
                c = await _client.CreateDocumentCollectionAsync(db.CollectionsLink,
                   new DocumentCollection()
                   {
                       Id = collectionId
                   });
            }

            return c;

        }


    }
}
