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

        public static string SpatialIndexPath { get; set; }

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

            if (db == null)
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
            var policy = GetIndexingPolicy();

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

                if (policy != null)
                {
                    c.IndexingPolicy = policy;
                }
            }
            else if (policy != null)
            {
                c.IndexingPolicy = policy;
                await _client.ReplaceDocumentCollectionAsync(c);

                long indexTransformationProgress = 0;

                while (indexTransformationProgress < 100)
                {
                    Console.WriteLine($"waiting for indexing to complete ({indexTransformationProgress})...");
                    ResourceResponse<DocumentCollection> response = await _client.ReadDocumentCollectionAsync(c.SelfLink);
                    indexTransformationProgress = response.IndexTransformationProgress;

                    await Task.Delay(TimeSpan.FromSeconds(1));

                }
            }

            return c;

        }

        private IndexingPolicy GetIndexingPolicy()
        {
            if (string.IsNullOrWhiteSpace(SpatialIndexPath))
            {
                return null;
            }

            var pol = new IndexingPolicy
            {
                IncludedPaths = new System.Collections.ObjectModel.Collection<IncludedPath>()
                {
                    new IncludedPath
                    {
                        Path = SpatialIndexPath,
                        Indexes = new System.Collections.ObjectModel.Collection<Index>()
                        {
                            new SpatialIndex(DataType.Point),
                            new RangeIndex(DataType.Number) {Precision = -1},
                            new RangeIndex(DataType.String) {Precision = -1}
                        },
                    },
                    new IncludedPath
                    {
                        Path = "/*",
                        Indexes = new System.Collections.ObjectModel.Collection<Index>()
                        {
                            new RangeIndex(DataType.Number) {Precision = -1},
                            new RangeIndex(DataType.String) {Precision = -1}
                        },
                    }
                }
            };

            return pol;
        }

    }
}
