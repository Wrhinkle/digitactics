using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;

internal static class MusicPlaylistChecks
{
    public static int Run(string root)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/music-playlist.json.gz"));
        using var gzip=new GZipStream(input,CompressionMode.Decompress); using var data=JsonDocument.Parse(gzip);
        var random=new Queue<double>(); var player=new MusicPlaylist(()=>random.Dequeue()); int checks=0;
        var options=new JsonSerializerOptions {PropertyNameCaseInsensitive=true};
        foreach(var row in data.RootElement.EnumerateArray())
        {
            player.SetTracks(row.GetProperty("tracks").Deserialize<PlaylistTrack[]>(options)!);
            foreach(var value in row.GetProperty("random").EnumerateArray())random.Enqueue(value.GetDouble());
            string phase=row.GetProperty("phase").GetString()!, action=row.GetProperty("action").GetString()!;
            // Phase is supplied independently for source next/failed calls as well.
            player.SetPhaseContext(phase);
            MusicTransition? transition=action switch
            {
                "start"=>player.Start(phase),"next"=>player.Next(),"phase"=>player.ChangePhase(phase),"stop"=>player.Stop(),"failure"=>player.PlaybackFailed(),"success"=>Success(),_=>throw new InvalidDataException()
            };
            MusicTransition? Success() { player.PlaybackStarted(); return null; }
            var expected=row.GetProperty("transition").Deserialize<MusicTransition>(options);
            checks++;
            if(transition!=expected || player.Current?.Id!=row.GetProperty("current").GetString() || player.Playing!=row.GetProperty("playing").GetBoolean() || player.Failed!=row.GetProperty("failed").GetBoolean() || player.Failures!=row.GetProperty("failures").GetInt32() || random.Count!=0)
                throw new InvalidOperationException($"Music playlist differed from source at {checks} ({action}): {transition} / {expected}.");
        }
        Console.WriteLine($"Music playlist: {checks} original-source selection, phase, failure and fade transitions passed.");return checks;
    }
}
