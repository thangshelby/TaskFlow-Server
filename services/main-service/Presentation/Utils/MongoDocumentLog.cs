using MongoDB.Bson;
using System.Text.Json;
public static class MongoDocumentLogUtil
{
    public static void LogObject(ILogger logger, dynamic data)
    {
        var json = JsonSerializer.Serialize((object)data, new JsonSerializerOptions { WriteIndented = true });
        logger.LogInformation("Dynamic:\n{Json}", json);
    }
    public static void LogBsonDocument(ILogger logger, BsonDocument document, string title = "MongoDB Document")
    {
        var cleaned = ConvertObjectIdsToStrings(document);
        var json = JsonSerializer.Serialize(cleaned, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        logger.LogInformation("{Title}: {Json}", title, json);
    }

    public static void LogBsonDocuments(ILogger logger, IEnumerable<BsonDocument> documents, string title = "MongoDB Documents")
    {
        var cleaned = documents.Select(ConvertObjectIdsToStrings).ToList();
        var json = JsonSerializer.Serialize(cleaned, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        logger.LogInformation("{Title}: {Json}", title, json);
    }

    private static Dictionary<string, object?> ConvertObjectIdsToStrings(BsonDocument doc)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var element in doc.Elements)
        {
            if (element.Value.IsObjectId)
            {
                dict[element.Name] = element.Value.AsObjectId.ToString();
            }
            else if (element.Value.IsBsonDocument)
            {
                dict[element.Name] = ConvertObjectIdsToStrings(element.Value.AsBsonDocument);
            }
            else if (element.Value.IsBsonArray)
            {
                dict[element.Name] = element.Value.AsBsonArray.Select(val =>
                    val.IsBsonDocument ? ConvertObjectIdsToStrings(val.AsBsonDocument) :
                    val.IsObjectId ? val.AsObjectId.ToString() :
                    BsonTypeMapper.MapToDotNetValue(val)
                ).ToList();
            }
            else
            {
                dict[element.Name] = BsonTypeMapper.MapToDotNetValue(element.Value);
            }
        }

        return dict;
    }
}
