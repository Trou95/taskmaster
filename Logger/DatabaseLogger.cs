using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Taskmaster.Logger;

public class DatabaseLogger : ILogger
{
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<BsonDocument> _collection;

   public DatabaseLogger()
    {
        var connectionString = "mongodb+srv://root:12345678a@taskmaster.lxy3gn4.mongodb.net/?retryWrites=true&w=majority&appName=taskmaster";

        _client = new MongoClient(connectionString);
        _database = _client.GetDatabase("taskmaster");
        _collection = _database.GetCollection<BsonDocument>("logs");
    }

    public void Log(string message)
    {
       _collection.InsertOne(BsonDocument.Parse(JsonSerializer.Serialize(new { Message = message, Time = DateTime.Now })));
    }

}
