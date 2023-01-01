//using MongoDB.Bson;
//using MongoDB.Driver;

//namespace TestMate.Runner.BackgroundServices
//{
//        public class MongoService__
//        {
//            public readonly IMongoClient _client;
//            public readonly IMongoDatabase _database;
//            public readonly IMongoCollection<BsonDocument> _collection;
            
//            public MongoService__(string collection)
//            {
//                _client = new MongoClient("mongodb://localhost:27017");
//                _database = _client.GetDatabase("test");
//                _collection = _database.GetCollection<BsonDocument>(collection);
                
//            }
//        }
//}
