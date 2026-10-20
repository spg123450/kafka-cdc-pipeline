using System.Net.Http.Json;
using System.Text.Json;

Console.WriteLine("=== DOWNSTREAM CONSUMER CLIENT ===");
Console.WriteLine("Connecting to Gateway API...");

using var client = new HttpClient();
string apiUrl = "http://localhost:5000/api/CDCTargetApi/events";
string exportFilePath = "ClientMedia_Export.json";

try
{
    // 1. Request the data from the external Gateway API
    Console.WriteLine($"Fetching real-time events from {apiUrl}...");
    var response = await client.GetAsync(apiUrl);
    response.EnsureSuccessStatusCode();

    // 2. Read the JSON response
    var rawJson = await response.Content.ReadAsStringAsync();

    // 3. Format it beautifully for the clients
    var parsedJson = JsonSerializer.Deserialize<object>(rawJson);
    var prettyJson = JsonSerializer.Serialize(parsedJson, new JsonSerializerOptions { WriteIndented = true });

    // 4. Save it locally to simulate "Client Media" storage
    await File.WriteAllTextAsync(exportFilePath, prettyJson);

    Console.WriteLine("\n[SUCCESS] Data successfully consumed from pipeline!");
    Console.WriteLine($"[FILE GENERATED] Saved to: {Path.GetFullPath(exportFilePath)}");
}
catch (Exception ex)
{
    Console.WriteLine($"\n[ERROR] Failed to consume data: {ex.Message}");
    Console.WriteLine("Ensure your CDCTarget API is running!");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadLine();