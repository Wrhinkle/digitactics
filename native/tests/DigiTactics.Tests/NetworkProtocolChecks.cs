using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Networking;

internal static class NetworkProtocolChecks
{
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/network-protocol.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int count = 0;
        foreach (var row in fixture.RootElement.EnumerateArray())
        {
            string wire = row.GetProperty("wire").GetString()!;
            bool expected = row.GetProperty("valid").GetBoolean();
            var parsed = NetworkProtocol.Parse(wire);
            if ((parsed is not null) != expected) throw new InvalidOperationException($"Wire validation expected {expected}: {wire}");
            if (parsed is { } message && message.GetProperty("l").ValueKind != JsonValueKind.String) throw new InvalidOperationException("Returned message lost its owned JSON data.");
            count++;
        }
        if (NetworkProtocol.Parse(new string(' ', NetworkProtocol.MaximumWire + 1)) is not null) throw new InvalidOperationException("Oversized payload accepted.");
        // JSON with an unpaired UTF-16 escape cannot be represented by the native UTF-8 transport.
        if (NetworkProtocol.Parse("{\"l\":\"999\",\"t\":\"presdel\",\"ignored\":\"\\ud800\"}") is not null) throw new InvalidOperationException("Malformed Unicode accepted.");
        Console.WriteLine($"Network protocol: {count + 2} original wire validation, payload-limit and Unicode checks passed.");
        return count + 2;
    }
}
