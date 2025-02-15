
Console.Write("Send to port: ");
var port = int.Parse(Console.ReadLine()!);

Console.Write("Period in ms: ");
var waitMs = int.Parse(Console.ReadLine()!);

Console.Write("numericId=");
var numericId = int.Parse(Console.ReadLine()!);

Console.Write("typeId=");
var typeId = Console.ReadLine();

using var http = new HttpClient();
var insertTime = DateTimeOffset.UtcNow;

while (true)
{
    try
    {
        var value = Random.Shared.Next(0, 40);
        var url = $"http://localhost:{port}/api/sensor/{numericId}/{typeId}?value={value}&measurementTime={Uri.EscapeDataString(insertTime.ToString("o"))}&quality=1";
        Console.WriteLine("Requesting: " + url);
        var resp = await http.PostAsync(url, null);
        resp.EnsureSuccessStatusCode();
        Console.WriteLine("SUCCESS");
    }
    catch (Exception e)
    {
        Console.WriteLine("FAIL");
        Console.WriteLine(e.ToString());
    }

    insertTime = insertTime.AddMinutes(1);
    await Task.Delay(waitMs);
}
