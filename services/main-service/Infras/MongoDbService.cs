using MongoDB.Driver;
using MongoDB.Bson;

namespace MainService.Infras;
public class MongoDbService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MongoDbService> _logger;
    private IMongoDatabase? _database;
    public MongoDbService(IConfiguration configuration, ILogger<MongoDbService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        Initiate();
    }

    public IMongoDatabase Database => _database ?? throw new InvalidOperationException("Database not initialized. Call Initiate() first.");

    public void Initiate()
    {
        try
        {
            var connectionStr = _configuration.GetConnectionString("MongoDb");
            var mongoUrl = MongoUrl.Create(connectionStr);
            var mongoClient = new MongoClient(mongoUrl);

            var dbName = mongoUrl.DatabaseName ?? "taskflow";
            _database = mongoClient.GetDatabase(dbName);

            var collection = _database.GetCollection<BsonDocument>("dummy_collection");
            var dummyDocument = new BsonDocument { { "initialized", true } };
            collection.InsertOne(dummyDocument);

            _logger.LogInformation("MongoDB connection successful!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"MongoDB connection failed: {ex.Message}");
        }
    }

}
