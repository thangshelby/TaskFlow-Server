using Google.Api;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
public static class MongoUtils
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

    public static BsonArray BuildExprMongo(
    object source,
    Dictionary<string, (string field, string op)> opMap,
    IEnumerable<string>? excludeProps = null)
    {
        var exprList = new BsonArray();
        var type = source.GetType();
        var excludeSet = excludeProps != null ? [.. excludeProps] : new HashSet<string>();

        foreach (var prop in type.GetProperties())
        {
            var propName = prop.Name;

            // bỏ field nếu trong excludeFields
            if (excludeSet.Contains(propName))
                continue;

            var value = prop.GetValue(source);
            if (value == null) continue;
            if (value is string s && string.IsNullOrWhiteSpace(s)) continue;
            if (value is System.Collections.IEnumerable e && value is not string)
            {
                bool any = false; foreach (var _ in e) { any = true; break; }
                if (!any) continue;
            }

            // lấy op + fieldName
            opMap.TryGetValue(propName, out var opField);
            var op = string.IsNullOrEmpty(opField.op) ? "$eq" : opField.op;
            var fieldName = string.IsNullOrEmpty(opField.field)
                ? propName // không convert nữa
                : opField.field;

            if (op == "$in" && value is IEnumerable<string> strList)
            {
                exprList.Add(new BsonDocument("$in",
                    new BsonArray { $"${fieldName}", new BsonArray(strList) }));
            }
            else if (op == "$regex" && value is string pattern)
            {
                exprList.Add(new BsonDocument("$regexMatch", new BsonDocument
            {
                { "input", $"${fieldName}" },
                { "regex", pattern },
                { "options", "i" }
            }));
            }
            else if (op == "$lte" || op == "$gte" || op == "$eq")
            {
                exprList.Add(new BsonDocument(op,
                    new BsonArray { $"${fieldName}", BsonValue.Create(value) }));
            }
        }

        return exprList;
    }

    public static UpdateDefinition<T> MakeMongoDataUpdate<T>(MongoUpdateInput input)
    {
        var data = ConverterUtils.MakeDataUpdate(new ConverterUtils.DataUpdateInput
        {
            Data = input.Data,
            RemoveFields = input.RemoveFields,
            RemoveValues = input.RemoveValues,
        });
        var updates = new List<UpdateDefinition<T>>();
        var builder = Builders<T>.Update;

        foreach (var kv in data)
        {
            updates.Add(builder.Set(kv.Key, kv.Value));
        }

        if (input.IsUpdatedAt)
        {
            updates.Add(builder.Set("UpdatedAt", DateTime.UtcNow));
        }

        return builder.Combine(updates);
    }

    public class MongoUpdateInput
    {
        public object Data { get; set; } = default!;
        public List<object?>? RemoveValues { get; set; } = null;
        public List<string>? RemoveFields { get; set; } = null;
        public bool IsUpdatedAt { get; set; } = true;
    }
}
