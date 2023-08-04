using PlebJournal.Db.Models;

namespace PlebJournal.Db.Seed;

using System.IO;
using System.Text.Json;

public static class SeedData
{
    public static List<Price> LoadSeedData()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Seed/historical-usd-prices.json");
        var json = File.ReadAllText(path);
        var camelCase = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var prices = JsonSerializer.Deserialize<List<Price>>(json, camelCase);
        return prices;
    }
}